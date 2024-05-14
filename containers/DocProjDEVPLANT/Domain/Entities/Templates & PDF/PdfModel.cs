namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfModel : Entity
{
    public byte[] Content { get; set; }
    
    public TemplateModel Template { get; set; }
}