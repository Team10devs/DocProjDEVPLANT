namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfModel : Entity
{
    public PdfModel(TemplateModel template)
    {
        CurrentNumberOfUsers = 0;
        Template = template;
        Content = [];
        Jsons = new List<string>();
    }
    
    private PdfModel()
    {
    }

    public byte[] Content { get; set; }
    public int CurrentNumberOfUsers { get; set; }
    public TemplateModel Template { get; set; }
    public List<string> Jsons { get; set; }
}