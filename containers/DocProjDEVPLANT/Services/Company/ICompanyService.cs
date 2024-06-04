using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Company;

public interface ICompanyService
{
    Task<Result<IEnumerable<CompanyModel>>> GetAllAsync();
    Task<Result<CompanyModel>> CreateCompanyAsync(CompanyRequest request);
    Task<Result<CompanyModel>> GetByIdAsync(string companyId);
    Task<Byte[]> MakePdfFromDictionay(string companyId, string templateId, Dictionary<string, string> dictionary);
    Task<byte[]> ConvertDocxToJson(string companyId, string templateName, IFormFile file);
    Task<PdfModel> GenerateEmptyPdf(string companyId, string templateId);
    Task<Result> AddUserToCompanyAsync(string companyId, string userId);
    Task<PdfModel> AddUserToPdf(string pdfId, string json);
}