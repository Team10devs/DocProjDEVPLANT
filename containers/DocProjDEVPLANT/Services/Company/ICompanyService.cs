using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Entities;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Company;

public interface ICompanyService
{
    Task<Result<IEnumerable<CompanyModel>>> GetAllAsync();
    Task<Result<CompanyModel>> CreateCompanyAsync(CompanyRequest request);
}