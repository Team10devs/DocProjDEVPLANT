namespace DocProjDEVPLANT.Entities;

public interface ICompanyRepository
{
    public Task<List<CompanyModel>> GetAllCompaniesAsync();
    public Task CreateCompanyAsync(CompanyModel companyModel);
    public Task<CompanyModel> FindByIdAsync(string id);
    public Task DeleteCompanyAsync(CompanyModel companyModel);
}