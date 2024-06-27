using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.User;

public interface IUserService
{
    Task<Result<IEnumerable<UserModel>>> GetAllAsync();
    Task<UserModel> GetUserByEmailAsync(string email);
    Task<UserModel> CreateUserAsync(UserRequest request);
    Task<Result<UserModel>> GetByIdAsync(string id);
    Task<List<UserModel>> GetUsersByCompanyAsync(string companyName);
    Task<Result> AddIdVariables(IFormFile image);

    Task<Result<UserModel>> AddIdVariablesToUser(string userId, UserPersonalData personalDataDto);
    Task<UserModel> UpdateUserJson(string userEmail, string jsonData);
    Task UpdateUserAsync(UserModel user);
}