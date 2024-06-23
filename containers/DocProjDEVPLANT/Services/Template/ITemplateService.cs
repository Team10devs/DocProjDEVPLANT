
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Template;

public interface ITemplateService
{
    Task<Result<TemplateModel>> GetTemplateById(string id);
    Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId);
    Task<TemplateModel> GetTemplatesByName(string name);//,string token);
    Task DeleteTemplateAsync(string templateId);
    Task<List<PdfResponseMinio>> GetPdfsByTemplateId(string templateId);
    Task<TemplateModel> GetTemplateByPdfId(string pdfId);
    Task<byte[]> PatchTemplate(string id, string newName, IFormFile file);
}