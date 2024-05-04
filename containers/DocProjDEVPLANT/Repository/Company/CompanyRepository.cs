using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.Company;

public class CompanyRepository(AppDbContext context) : Repository<CompanyModel>(context), ICompanyRepository
{
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

    public async Task UploadDocument(string companyId, string templateId, byte[] document)
    {
        var template = await _appDbContext.Templates
            .Include(t=>t.GeneratedPdfs)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template is null)
            throw new Exception($"Company with id {companyId} does not have a template with id {templateId}");

        var pdfModel = new PdfModel();
        pdfModel.Content = document;
        pdfModel.Template = template;
        
        template.GeneratedPdfs.Add(pdfModel);
        await _appDbContext.SaveChangesAsync();
    }
}