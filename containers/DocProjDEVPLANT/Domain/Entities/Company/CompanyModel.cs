using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Entities;

public class CompanyModel : Entity
{
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }
    //list of templates

    private CompanyModel() { }

    public static async Task<Result<CompanyModel>> CreateAsync(
        ICompanyRepository repo,
        string name)
    {
        return new CompanyModel
        {
            Name = name
        };
    }

}