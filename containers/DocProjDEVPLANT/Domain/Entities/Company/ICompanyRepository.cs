namespace DocProjDEVPLANT.Entities;

public interface ICompanyRepository
{
    public Task<List<CompanyModel>> GetAllCompaniesAsync();
    public Task CreateCompanyAsync(CompanyModel companyModel);
    public Task<CompanyModel> FindById(string id);
}