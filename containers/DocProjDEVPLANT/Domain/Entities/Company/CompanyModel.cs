using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.Company;

public class CompanyModel : Entity
{
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }
    //list of templates

    private CompanyModel() { }

    public CompanyModel(string name, List<UserModel> users)
    {
        Name = name;
        Users = users;
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