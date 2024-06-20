
using DocProjDEVPLANT.API.DTOs.Template;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using PdfResponse = DocProjDEVPLANT.Domain.Entities.Templates.PdfResponse;

namespace DocProjDEVPLANT.Services.Template;

public interface ITemplateService
{
    Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId);
    Task<TemplateModel> GetTemplatesByName(string name);//,string token);
    Task DeleteTemplateAsync(string templateId);
    Task<List<PdfResponseMinio>> GetPdfsByTemplateId(string templateId);
}