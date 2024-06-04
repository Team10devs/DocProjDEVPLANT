namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfModel : Entity
{
    public PdfModel(TemplateModel template)
    {
        CurrentNumberOfUsers = 0;
        Template = template;
        Content = [];
    }
    
    private PdfModel()
    {
    }

    public byte[] Content { get; set; }
    public int CurrentNumberOfUsers { get; set; }
    public TemplateModel Template { get; set; }
}