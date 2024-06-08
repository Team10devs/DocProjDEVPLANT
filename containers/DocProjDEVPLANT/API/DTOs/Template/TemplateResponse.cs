namespace DocProjDEVPLANT.API.DTOs.Template;

public record TemplateResponse(string Id,string TemplateName, string CompanyName, int TotalNumberOfUsers/*byte[] docxfile (e imens)*/);