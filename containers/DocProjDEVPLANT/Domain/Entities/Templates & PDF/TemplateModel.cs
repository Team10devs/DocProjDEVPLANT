namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class TemplateModel : Entity
{
    public string Name { get; set; }
    public byte[] DocxFile { get; set; }
    public List<PdfModel> GeneratedPdfs { get; set; }
}