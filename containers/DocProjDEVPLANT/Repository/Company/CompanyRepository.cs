using System.Text.Json;
using System.Text.RegularExpressions;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Mail;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocProjDEVPLANT.Repository.Company;

public class CompanyRepository :  ICompanyRepository
{
    
    protected readonly AppDbContext _appDbContext;
    protected readonly IEmailService _emailService;
    public CompanyRepository(AppDbContext appDbContext, IEmailService emailService)
    {
        _appDbContext = appDbContext;
        _emailService = emailService;
    }
    public async Task<List<CompanyModel>> GetAllCompaniesAsync()
    {
        return await _appDbContext.Companies
            .Include(c => c.Users)
            .Include(t=>t.Templates)
            .ToListAsync();
    }

    public async Task CreateCompanyAsync(CompanyModel companyModel)
    {
        _appDbContext.Add(companyModel);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<CompanyModel> FindByIdAsync(string id)
    {
        return await _appDbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<TemplateModel> FindByIdWithTemplateAsync(string companyId, string templateId)
    {
        var company = await _appDbContext.Companies
            .Include(c => c.Templates)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company is null)
            throw new Exception($"Company with id {companyId} does not exist.");

        var template = company.Templates.FirstOrDefault(t => t.Id == templateId);

        if (template is null)
            throw new Exception($"Company with id {companyId} does not have a template with id {templateId}");
        
        return template;
    }

    public async Task<PdfModel> GenerateEmptyPdf(string companyId, string templateId)
    {
        TemplateModel template;
        try
        {
            template = await FindByIdWithTemplateAsync(companyId, templateId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        
        var pdf = new PdfModel(template);

        await _appDbContext.Pdfs.AddAsync(pdf);
        await _appDbContext.SaveChangesAsync();
        
        return pdf;
    }

    public async Task DeleteCompanyAsync(CompanyModel companyModel)
    {
        var company = await _appDbContext.Companies.FindAsync(companyModel.Id);
        
        if (company is null) return;
        
        _appDbContext.Companies.Remove(company);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task<bool> UpdateAsync(CompanyModel company)
    {
        _appDbContext.Companies.Update(company);
        var affectedRows = await _appDbContext.SaveChangesAsync();
        return affectedRows > 0;
    }

    public async Task UploadDocument(string companyId, string templateId, byte[]? document)
    {
        var template = await _appDbContext.Templates
            .Include(t=>t.GeneratedPdfs)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template is null)
            throw new Exception($"Company with id {companyId} does not have a template with id {templateId}");

        var pdfModel = new PdfModel(template);
        pdfModel.Content = document;
        
        template.GeneratedPdfs.Add(pdfModel);
        await _appDbContext.SaveChangesAsync();
    }
    
    public static bool ValidateJson(string templateJson, string pdfDataJson)
    {
        JObject templateObj = JObject.Parse(templateJson);
        JObject pdfDataObj = JObject.Parse(pdfDataJson);
        
        foreach (var property in templateObj.Properties())
        {
            if (!ValidateProperty(property, pdfDataObj))
            {
                return false;
            }
        }
        
        return true;
    }

    private static bool ValidateProperty(JProperty property, JObject pdfDataObj)
    {
        JToken pdfDataToken;
        if (!pdfDataObj.TryGetValue(property.Name, out pdfDataToken))
        {
            // Key not found in pdfData
            return false;
        }

        if (property.Value.Type == JTokenType.Object)
        {
            if (pdfDataToken.Type != JTokenType.Object)
            {
                // Types do not match
                return false;
            }

            // Iterate over nested object properties
            foreach (var nestedProperty in ((JObject)property.Value).Properties())
            {
                if (!ValidateProperty(nestedProperty, (JObject)pdfDataToken))
                {
                    return false;
                }
            }
        }
        else
        {
            // Check if value is not empty (null, empty string, etc.)
            if (pdfDataToken.Type == JTokenType.Null || 
                (pdfDataToken.Type == JTokenType.String && string.IsNullOrEmpty((string)pdfDataToken)))
            {
                // Value is empty
                return false;
            }
        }

        return true;
    }

    public async Task<PdfModel> AddUserToPdf(string pdfId, string userEmail, string json)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .Include(p=>p.Users)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception($"The json file must not be null");
        
        if (!Regex.IsMatch(userEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$"))
            throw new Exception($"The email {userEmail} is not a valid email");
        
        var user = await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        var templateJson = pdf.Template.JsonContent;
        
        //verifica daca e ok json ul
        if (!ValidateJson(templateJson, json))
        {
            throw new Exception($"The json is not valid");
        }
        
        if (user is null)
        {
            var registerLink = $"http://localhost:3000/register";
            
            var result = await UserModel.CreateAsync(
                userEmail,
                RoleEnum.UnregisteredUser);

            if (result.IsFailure)
                throw new Exception(result.Error.ToString());
            
            user = result.Value;
            
            _appDbContext.Add(user);
            await _appDbContext.SaveChangesAsync();

            await _emailService.SendRegisterEmailAsync(user.Email, registerLink);
        }
        
        try
        {
            
            pdf.CurrentNumberOfUsers++;
            pdf.Jsons.Add(json);
            pdf.Users.Add(user);

            if (user.Role == RoleEnum.UnregisteredUser)
            {
                JObject userDataObject = JObject.Parse(user.UserData);
                    
                user.Address = (string?)userDataObject["client"]["adresa"];
                user.FullName = (string?)userDataObject["client"]["nume"];
                user.Country = (string?)userDataObject["client"]["tara"];
                user.Cetatenie = (string?)userDataObject["client"]["cetatenie"];
                user.Sex = (string?)userDataObject["client"]["sex"];
                user.Judet = (string?)userDataObject["client"]["localitate"];
                user.CNP = (string?)userDataObject["client"]["cnp"];
            }
            
            if (string.IsNullOrWhiteSpace(user.UserData))
            {
                user.UserData = json;
            }
            else
            {
                var originalUserData = JObject.Parse(user.UserData);
                var newUserData = JObject.Parse(json);
                
                originalUserData.Merge(newUserData, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

                user.UserData = originalUserData.ToString(Formatting.None); // se poate modifica de aici formatarea
            }

            _appDbContext.Pdfs.Update(pdf);
            _appDbContext.Users.Update(user);
            
            await _appDbContext.SaveChangesAsync();
            
            return pdf;
            
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<(PdfModel, TemplateModel)> VerifyNumberOfUsers(string pdfId, string templateId)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .Include(p=>p.Users)
            .FirstOrDefaultAsync(p => p.Id == pdfId);
        
        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        var template = await _appDbContext.Templates
            .Include(t => t.GeneratedPdfs)
            .Include(t=>t.Company)
            .FirstOrDefaultAsync(t => t.Id == templateId);
        
        if (template is null)
            throw new Exception($"Template with id {templateId} does not exist");

        if (pdf.Template.Id != templateId)
            throw new Exception($"The pdf does not correspond to the template");

        if (pdf.CurrentNumberOfUsers < template.TotalNumberOfUsers)
            throw new Exception($"Not enough users have completed their forms {pdf.CurrentNumberOfUsers}/{template.TotalNumberOfUsers}");
        
        if (pdf.CurrentNumberOfUsers > template.TotalNumberOfUsers)
            throw new Exception($"More users than required have completed their forms");
        
        return (pdf, template);
    }

    public async Task AddContentToPdf(string pdfId, byte[] byteArray)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");
        
        pdf.Content = byteArray;
        _appDbContext.Pdfs.Update(pdf);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task AddTemplate (string companyId, string templateName, byte[] fileContent, int totalNumberOfUsers,string jsonContent)
    {
        var company = await _appDbContext.Companies.Include(c => c.Templates)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company is null)
            throw new Exception( $"Company with id {companyId} does not exist.");

        var template = new TemplateModel(templateName, fileContent, company, totalNumberOfUsers,jsonContent);
        
        company.Templates.Add(template);

        _appDbContext.Templates.Add(template);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task SendEmailToUsers(UserModel user, TemplateModel templateModel, byte[] pdf)
    {
        await _emailService.SendEmailAsync(user, templateModel, pdf);
    }
}