using DocProjDEVPLANT.Domain.Entities.User;

namespace DocProjDEVPLANT.Repository.User;

public interface IUserRepository
{
    public Task<List<UserModel>> GetAllUsersAsync();
    public Task<UserModel> FindByIdAsync(string id);
    public Task<UserModel> FindByEmailAsync(string email);
    Task<bool> IsEmailUnique(string email);
    public Task CreateUserAsync(UserModel userModel);
    public Task DeleteUserAsync(UserModel userModel);
    Task<List<UserModel>> GetUsersByCompanyAsync(string companyName);
    public Task UpdateUserAsync(UserModel userModel);
}