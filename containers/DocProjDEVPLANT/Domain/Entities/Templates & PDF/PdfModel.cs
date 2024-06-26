using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfModel : Entity
{
    public PdfModel(TemplateModel template)
    {
        CurrentNumberOfUsers = 0;
        Template = template;
        Jsons = new List<string>();
        Users = new List<UserModel>();
    }
    
    private PdfModel()
    {
    }
    
    public int CurrentNumberOfUsers { get; set; }
    public TemplateModel Template { get; set; }
    public List<string> Jsons { get; set; }
    public List<UserModel> Users { get; set; }
    public PdfStatus Status { get; set; }
}