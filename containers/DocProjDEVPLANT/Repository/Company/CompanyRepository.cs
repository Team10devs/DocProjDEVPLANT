using System.Text.Json;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.Company;

public class CompanyRepository :  ICompanyRepository
{
    
    protected readonly AppDbContext _appDbContext;
    
    public CompanyRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
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

    public async Task<PdfModel> AddUserToPdf(string pdfId, string json)
    {
        var pdf = await _appDbContext.Pdfs
            .Include(p=>p.Template)
            .FirstOrDefaultAsync(p => p.Id == pdfId);

        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception($"The json file must not be null");

        try
        {
            using (var jsonDoc = JsonDocument.Parse(json))
            {
                pdf.CurrentNumberOfUsers++;
                pdf.Jsons.Add(json);
                
                _appDbContext.Pdfs.Update(pdf);
                await _appDbContext.SaveChangesAsync();
                
                return pdf;
            }
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
            .FirstOrDefaultAsync(p => p.Id == pdfId);
        
        if (pdf is null)
            throw new Exception($"Pdf with id {pdfId} does not exist");

        var template = await _appDbContext.Templates
            .Include(t => t.GeneratedPdfs)
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
    
    public async Task AddTemplate (string companyId, string templateName, byte[] fileContent, int totalNumberOfUsers)
    {
        var company = await _appDbContext.Companies.Include(c => c.Templates)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company is null)
            throw new Exception( $"Company with id {companyId} does not exist.");

        var template = new TemplateModel(templateName, fileContent, company, totalNumberOfUsers);
        
        company.Templates.Add(template);

        _appDbContext.Templates.Add(template);
        await _appDbContext.SaveChangesAsync();
    }
}