using DocProjDEVPLANT.Entities;
using DocProjDEVPLANT.Entities.Enums;

namespace DocProjDEVPLANT.Controllers;

public record UserResponse(
    string Id,
    string Username,
    string Fullname,
    string Cnp,
    RoleEnum Role,
    CompanyModel Company
    );