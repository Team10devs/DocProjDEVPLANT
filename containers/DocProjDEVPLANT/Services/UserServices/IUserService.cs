using DocProjDEVPLANT.Controllers;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services;

public interface IUserService
{
    Task<Result<IEnumerable<UserModel>>> GetAllAsync();

    Task<Result<UserModel>> CreateUserAsync(UserRequest request);
}