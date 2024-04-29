using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Domain.Entities.Company;

public class CompanyModel : Entity
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public RoleEnum Role { get; set; }
    public List<UserModel> Users { get; set; }
    //list of templates

    private CompanyModel() { }

    public CompanyModel(string userName,string password, string email,
            string name, List<UserModel> users)
    {
        UserName = userName;
        Password = password;
        Email = email;
        Name = name;
        Users = users;
    }
    
    public static async Task<Result<CompanyModel>> CreateAsync(
        ICompanyRepository repo,
        string username,
        string password,
        string email,
        RoleEnum role)
    {
        return new CompanyModel
        {
            UserName = username,
            Password = password,
            Email = email,
            Role = role,
        };
    }
}