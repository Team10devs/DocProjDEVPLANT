using DocProjDEVPLANT.Entities.User;

namespace DocProjDEVPLANT.Controllers.Companies;

public record CompanyResponse(
    string Id,
    string Name,
    List<UserModel> Users
    );