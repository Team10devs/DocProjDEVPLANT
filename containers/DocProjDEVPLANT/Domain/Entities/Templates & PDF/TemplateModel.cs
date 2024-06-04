using DocProjDEVPLANT.Domain.Entities.Company;

namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class TemplateModel : Entity
{
    public string Name { get; set; }
    public byte[] DocxFile { get; set; }
    public List<PdfModel> GeneratedPdfs { get; set; }
    public CompanyModel Company { get; set; }
    public int TotalNumberOfUsers { get; set; }

    
    private TemplateModel() {}
    public TemplateModel(string name, byte[] docxFile, CompanyModel companyModel, int totalNumberOfUsers)
    {
        Name = name;
        DocxFile = docxFile;
        Company = companyModel;
        TotalNumberOfUsers = totalNumberOfUsers;
    }
}