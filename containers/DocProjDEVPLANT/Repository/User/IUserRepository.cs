using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.Repository.User;

public interface IUserRepository
{
    public Task<List<UserModel>> GetAllUsersAsync();
    public Task<UserModel> FindByIdAsync(string id);
    public Task CreateUserAsync(UserModel userModel);
    public Task DeleteUserAsync(UserModel userModel);
}