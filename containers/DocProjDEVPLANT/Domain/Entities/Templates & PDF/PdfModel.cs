namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class PdfModel
{
    public byte[] Content { get; set; }
    
    public TemplateModel Template { get; set; }
}