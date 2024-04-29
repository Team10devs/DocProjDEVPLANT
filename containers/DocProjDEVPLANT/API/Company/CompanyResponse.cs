using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.API.Company;

public record CompanyResponse(string Id,string Name,List<UserModel> Users);