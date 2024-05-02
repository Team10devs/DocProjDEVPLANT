using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.API.Company;

public class CompanyResponseWithUsers
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<UserModel> Users { get; set; }
}