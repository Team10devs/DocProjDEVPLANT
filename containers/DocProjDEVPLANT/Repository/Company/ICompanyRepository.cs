using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;

namespace DocProjDEVPLANT.Repository.Company;

public interface ICompanyRepository
{
    public Task<List<CompanyModel>> GetAllCompaniesAsync();
    public Task CreateCompanyAsync(CompanyModel companyModel);
    public Task<CompanyModel> FindByIdAsync(string id);
    public Task<TemplateModel> FindByIdWithTemplateAsync(string id, string templateId);
    public Task DeleteCompanyAsync(CompanyModel companyModel); 
    public Task SaveChangesAsync();

    Task<bool> UpdateAsync(CompanyModel company);
    Task UploadDocument(string companyId, string templateId, byte[] document);
}