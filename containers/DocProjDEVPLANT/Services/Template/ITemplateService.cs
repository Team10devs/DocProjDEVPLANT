
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.Template;

public interface ITemplateService
{
    Task<Result<TemplateModel>> GetTemplateById(string id);
    Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByCompanyId(string companyId);
    Task<TemplateModel> GetTemplatesByName(string name);
    Task DeleteTemplateAsync(string templateId);
    Task<List<string>> GetPdfsByTemplateId(string templateId);
    Task<bool> EditTemplate(string id,string name, byte[] docx, int nrUsers);
}