using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public record UserResponse(
    string Id,
    string Username,
    string Fullname,
    string Cnp,
    RoleEnum Role,
    CompanyModel Company
    );