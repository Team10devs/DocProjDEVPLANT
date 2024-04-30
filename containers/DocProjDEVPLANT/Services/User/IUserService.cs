using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.User;

public interface IUserService
{
    Task<IEnumerable<UserModel>> GetAllAsync();

    Task<UserModel> CreateUserAsync(UserRequest request);
    Task<UserModel> GetByIdAsync(string id);
}