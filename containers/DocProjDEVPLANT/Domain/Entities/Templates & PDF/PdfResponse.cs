namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfResponse
{
    public string Id { get; set; }
    public string TemplateId { get; set; }
    public string TemplateName { get; set; }
    public int CurrentNumberOfUsers { get; set; }
    public List<string> Jsons { get; set; }
}