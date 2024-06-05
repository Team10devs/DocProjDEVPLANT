using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.Templates;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.Company;

public class CompanyModel : Entity
{
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }

    public List<TemplateModel> Templates { get; set; }

    private CompanyModel() { }

    public CompanyModel(string name, List<UserModel> users)
    {
        Name = name;
        Users = users;
        Templates = new List<TemplateModel>();
    }
    
    public static async Task<Result<CompanyModel>> CreateAsync(
        ICompanyRepository repo,
        string name
        )
    {
        return new CompanyModel
        {
            Name = name
        };
    }
}