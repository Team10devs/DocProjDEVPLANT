using DocProjDEVPLANT.Entities.Enums;

namespace DocProjDEVPLANT.Controllers;

public record UserResponse(
    string Id,
    string UserName,
    string Password,
    string Email,
    RoleEnum Role
    );