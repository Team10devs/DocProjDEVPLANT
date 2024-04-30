using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Company;

public interface ICompanyService
{
    Task<IEnumerable<CompanyModel>> GetAllAsync();
    Task<CompanyModel> CreateCompanyAsync(CompanyRequest request);
    Task<CompanyModel> GetByIdAsync(string companyId);
}