using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Company;

public interface ICompanyService
{
    Task<Result<IEnumerable<CompanyModel>>> GetAllAsync();
    Task<Result<CompanyModel>> CreateCompanyAsync(CompanyRequest request);
    Task<Result<CompanyModel>> GetByIdAsync(string companyId);

    Task<Result> AddTemplateToCompanyAsync(string companyId, string templateName, byte[] fileContent);
}