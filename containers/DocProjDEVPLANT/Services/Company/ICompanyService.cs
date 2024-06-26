using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Company;

public interface ICompanyService
{
    Task<Result<IEnumerable<CompanyModel>>> GetAllAsync();
    Task<CompanyModel> GetCompanyByNameAsync(string companyName);
    Task<CompanyModel> CreateCompanyAsync(CompanyRequest request);
    Task<Result<CompanyModel>> GetByIdAsync(string companyId);
    Task<Byte[]> GeneratePdf(string pdfId);
    Task<Byte[]> PreviewPdf(string pdfId, List<string> jsons);
    Task<byte[]> ConvertDocxToJson(string companyId, string templateName, IFormFile file);
    Task<PdfModel> GenerateEmptyPdf(string companyId, string templateId);
    Task<Result> AddUserToCompanyAsync(string companyId, string userId);
    Task<PdfModel> AddUserToPdf(string pdfId, string userEmail, string json,string? token = null);
    Task<PdfModel> PatchPdfJsons(string pdfId, List<string> jsons);
}