using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Minio;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xceed.Words.NET;

namespace DocProjDEVPLANT.Repository.Company;

public class CompanyRepository :  ICompanyRepository
{
    
    protected readonly AppDbContext _appDbContext;
    protected readonly IEmailService _emailService;
    protected readonly IMinioService _minioService;
    public CompanyRepository(AppDbContext appDbContext, IEmailService emailService,IMinioService minioService)
    {
        _appDbContext = appDbContext;
        _emailService = emailService;
        _minioService = minioService;
    }
    public async Task<List<CompanyModel>> GetAllCompaniesAsync()
    {
        return await _appDbContext.Companies
            .Include(c => c.Users)
            .Include(t=>t.Templates)
            .ToListAsync();
    }
    public async Task<CompanyModel> GetByNameAsync(string companyName)
    {
        return await _appDbContext.Companies.FirstOrDefaultAsync(c => c.Name == companyName);
    }

    public async Task<CompanyModel> GetByUserEmail(string userEmail)
    {
        var company = await _appDbContext.Companies
            .Include(c => c.Users)
            .Include(c => c.Templates)
            .FirstOrDefaultAsync(c => c.Users.Any(u => u.Email == userEmail));

        if (company is null)
            throw new Exception($"Company with userEmail {userEmail} does not exist");

        return company;
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
        pdf.Status = PdfStatus.Empty;

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

    /*public async Task UploadDocument(string companyId, string templateId, byte[]? document)
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
    } nu era folosita nici unde metoda asta*/
    
    public static bool ValidateJson(string templateJson, string pdfDataJson)
    {
        JObject templateObj;
        JObject pdfDataObj;
        try
        {
            templateObj = JObject.Parse(templateJson);
            pdfDataObj = JObject.Parse(pdfDataJson);
        }
        catch (Exception e)
        {
            throw new Exception($"Not a valid json format.");
        }
        
        
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
            .ThenInclude(t => t.Company)
            .Include(p=>p.Users)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        if (pdf.Status == PdfStatus.Completed)
            throw new Exception("This document has already been marked as completed!");

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception($"The json file must not be null");
        
        if (!Regex.IsMatch(userEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$"))
            throw new Exception($"The email {userEmail} is not a valid email");
        
        var user = await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        var templateJson = pdf.Template.JsonContent;
        
        // verifica daca e ok json ul
         if (!ValidateJson(templateJson, json))
         {
             throw new Exception($"The json is not valid");
         }
        
        if (user is null)
        {
            var registerLink = $"http://localhost:4200/register-page";
            
            var result = await UserModel.CreateAsync(
                userEmail,
                userEmail,
                RoleEnum.UnregisteredUser);

            if (result.IsFailure)
                throw new Exception(result.Error.ToString());
            
            user = result.Value;
            
            _appDbContext.Add(user);
            await _appDbContext.SaveChangesAsync();

            await _emailService.SendRegisterEmailAsync(user.Email,pdf.Template, registerLink);
        }
        
        try
        {
            pdf.Status = PdfStatus.InCompletion;
            pdf.CurrentNumberOfUsers++;
            pdf.Jsons.Add(json);
            pdf.Users.Add(user);

            if (pdf.Template.TotalNumberOfUsers == pdf.CurrentNumberOfUsers)
                pdf.Status = PdfStatus.Completed;

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

    public async Task<PdfModel> CheckPDF(string pdfId)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .Include(p=>p.Users)
            .Include(p=>p.Template.Company)
            .FirstOrDefaultAsync(p => p.Id == pdfId);
        
        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");
        
        if (pdf.CurrentNumberOfUsers < pdf.Template.TotalNumberOfUsers)
            throw new Exception($"Not enough users have completed their forms {pdf.CurrentNumberOfUsers}/{pdf.Template.TotalNumberOfUsers}");
        
        if (pdf.CurrentNumberOfUsers > pdf.Template.TotalNumberOfUsers)
            throw new Exception($"More users than required have completed their forms");

        return pdf;
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

    public async Task<PdfModel> UpdatePdfJsons(string pdfId, List<string> jsons)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .Include(p=>p.Users)
            .FirstOrDefaultAsync(p => p.Id == pdfId);
        
        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        if (pdf.CurrentNumberOfUsers != jsons.Count)
            throw new Exception($"The number of json strings given is not correct");
        
        var templateJson = pdf.Template.JsonContent;
        
        // verifica daca e ok json ul
        foreach (var json in jsons)
        {
            if (!ValidateJson(templateJson, json))
            {
                throw new Exception($"The json {json} is not valid for template {templateJson}");
            }   
        }

        pdf.Jsons = jsons;
        _appDbContext.Pdfs.Update(pdf);
        await _appDbContext.SaveChangesAsync();
        
        await GeneratePdfBytesAndUpdateAsync(pdf); // regenerare pdf
        return pdf;
    }
    
    
private async Task<byte[]> GeneratePdfBytesAndUpdateAsync(PdfModel pdf)
{
    var templateDocxBytes = pdf.Template.DocxFile;

    if (templateDocxBytes == null || templateDocxBytes.Length == 0)
    {
        throw new Exception("Not a docx file.");
    }

    var jsonList = pdf.Jsons.Select(JObject.Parse).ToList();

    using (var stream = new MemoryStream(templateDocxBytes))
    {
        stream.Seek(0, SeekOrigin.Begin);
        using (var doc = DocX.Load(stream))
        {
            string pattern = @"\{\{([^{}]+)\}\}";
            var replacedValues = new Dictionary<string, string>();

            foreach (var paragraph in doc.Paragraphs)
            {
                var matches = Regex.Matches(paragraph.Text, pattern);

                foreach (Match match in matches)
                {
                    var cleanedWord = match.Value.TrimStart('{').TrimEnd('}');
                    var parts = cleanedWord.Split('.');

                    if (parts.Length == 2)
                    {
                        var primaryKey = parts[0].TrimStart();
                        var secondaryKey = parts[1].TrimEnd();

                        if (replacedValues.TryGetValue(cleanedWord, out var replacedValue))
                        {
                            paragraph.ReplaceText(match.Value, replacedValue);
                            continue;
                        }

                        var numberPart = Regex.Match(primaryKey, @"\d+$");
                        JObject jsonObject = null;
                        if (numberPart.Success)
                        {
                            primaryKey = Regex.Replace(primaryKey, @"\d+$", "").TrimEnd();

                            var number = int.Parse(numberPart.Value);
                            if (number > 0 && number <= pdf.CurrentNumberOfUsers)
                            {
                                jsonObject = jsonList[number - 1];
                            }
                        }

                        if (jsonObject == null)
                        {
                            for (int i = pdf.CurrentNumberOfUsers - 1; i >= 0; i--)
                            {
                                var jsonListObject = jsonList[i];
                                if (jsonListObject.TryGetValue(primaryKey, out JToken primaryToken1) &&
                                    primaryToken1 is JObject primaryObject1 &&
                                    primaryObject1.TryGetValue(secondaryKey, out JToken secondaryToken1) &&
                                    secondaryToken1.Type != JTokenType.Null)
                                {
                                    jsonObject = jsonListObject;
                                    break;
                                }
                            }
                        }

                        if (jsonObject != null && 
                            jsonObject.TryGetValue(primaryKey, out JToken primaryToken) &&
                            primaryToken is JObject primaryObject &&
                            primaryObject.TryGetValue(secondaryKey, out JToken secondaryToken) && 
                            secondaryToken.Type != JTokenType.Null)
                        {
                            paragraph.ReplaceText(match.Value, secondaryToken.ToString());
                            replacedValues[cleanedWord] = secondaryToken.ToString();
                            primaryObject[secondaryKey] = null;
                        }
                        else
                        {
                            throw new Exception($"The json {jsonObject} does not have the value for {primaryKey}.{secondaryKey}");
                        }
                    }
                }
            }

            var modifiedStream = new MemoryStream();
            doc.SaveAs(modifiedStream);
            var docxBytes = modifiedStream.ToArray();

            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".docx");
            await File.WriteAllBytesAsync(tempFilePath, docxBytes);

            var pdfFilePath = Path.ChangeExtension(tempFilePath, ".pdf");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = $"--convert-to pdf {tempFilePath} --headless",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                await process.WaitForExitAsync();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Error converting DOCX to PDF: {error}");
                }

                var pdfBytes = await File.ReadAllBytesAsync(pdfFilePath);
                
                await _minioService.RemoveFileAsync("pdf-bucket", $"{pdf.Id}.pdf");
                
                await _minioService.UploadFileAsync("pdf-bucket", $"{pdf.Id}.pdf", pdfFilePath, pdf.Template.Name);

                File.Delete(tempFilePath);
                File.Delete(pdfFilePath);
                
                return pdfBytes;
            }
        }
    }
}

}