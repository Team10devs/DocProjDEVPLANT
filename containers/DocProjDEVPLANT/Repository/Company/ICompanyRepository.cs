using DocProjDEVPLANT.Domain.Entities.Company;

namespace DocProjDEVPLANT.Repository.Company;

public interface ICompanyRepository
{
    public Task<List<CompanyModel>> GetAllCompaniesAsync();
    public Task CreateCompanyAsync(CompanyModel companyModel);
    public Task<CompanyModel> FindByIdAsync(string id);
    public Task DeleteCompanyAsync(CompanyModel companyModel); 
    public Task SaveChangesAsync();

    Task<bool> UpdateAsync(CompanyModel company);
}