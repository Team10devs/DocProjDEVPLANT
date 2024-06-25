using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfResponse
{
    public string Id { get; set; }
    public string TemplateId { get; set; }
    public string TemplateName { get; set; }
    public int CurrentNumberOfUsers { get; set; }
    public List<string> Jsons { get; set; }
    public List<UserResponse> Users { get; set; }
    public PdfStatus Status { get; set; }
}