using DocProjDEVPLANT.Controllers;
using DocProjDEVPLANT.Controllers.Companies;
using DocProjDEVPLANT.Entities.Company;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.CompanyServices;

public interface ICompanyService
{
    Task<Result<IEnumerable<CompanyModel>>> GetAllAsync();

    Task<Result<CompanyModel>> CreateCompanyAsync(CompanyRequest request);
}