using Newtonsoft.Json.Linq;

namespace DocProjDEVPLANT.API.DTOs.Template;

public record TemplateResponse(string Id,string TemplateName, string CompanyName, int TotalNumberOfUsers,string? jsonContent = null/*byte[] docxfile (e imens)*/);