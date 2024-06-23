using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.Domain.Entities.Templates;

public class TemplateModel : Entity
{
    public string Name { get; set; }
    public byte[] DocxFile { get; set; }
    public List<PdfModel> GeneratedPdfs { get; set; }
    public CompanyModel Company { get; set; }
    public int TotalNumberOfUsers { get; set; }
    public string JsonContent { get; set; }
    
    private TemplateModel() {}
    public TemplateModel(string name, byte[] docxFile, CompanyModel companyModel, int totalNumberOfUsers,string jsonContent)
    {
        Name = name;
        DocxFile = docxFile;
        Company = companyModel;
        TotalNumberOfUsers = totalNumberOfUsers;
        JsonContent = jsonContent;
    }

    public bool HasFilledPdfs()
    {
        foreach ( PdfModel pdf in GeneratedPdfs )
        {
            if (pdf.Status == PdfStatus.Completed)
                return true;
        }

        return false;
    }
}