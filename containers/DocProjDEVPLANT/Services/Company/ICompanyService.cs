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
    Task<Byte[]> MakePdfFromDictionay(string companyId, string templateId, Dictionary<string, string> dictionary);
    Task<List<Input>> MakeInputListFromDocx(string companyId, string templateName, IFormFile file);
    Task<Result> AddUserToCompanyAsync(string companyId, string userId);
}