using DocProjDEVPLANT.Domain.Entities.Enums;

namespace DocProjDEVPLANT.API.User;

public record UserRequest(string email,string username,RoleEnum role);