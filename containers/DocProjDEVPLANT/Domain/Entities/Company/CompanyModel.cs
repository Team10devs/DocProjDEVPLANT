using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.Company;

public class CompanyModel : Entity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }
    //list of templates

    private CompanyModel() { }

    public CompanyModel(string userName,string password, string email,
            string name, List<UserModel> users)
    {
        Email = email;
        Name = name;
        Users = users;
    }
    
    public static async Task<Result<CompanyModel>> CreateAsync(
        ICompanyRepository repo,
        string email,
        string name
        )
    {
        return new CompanyModel
        {
            Email = email,
            Name = name
        };
    }
}