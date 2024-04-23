using DocProjDEVPLANT.Controllers;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository repository)
    {
        _userRepository = repository;
    }

    public async Task<Result<IEnumerable<UserModel>>> GetAllAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<Result<UserModel>> CreateUserAsync(UserRequest request)
    {

        var result = await UserModel.CreateAsync(
            _userRepository,
            request.firstname,
            request.lastname);
        
        if (result.IsFailure)
            return Result.Failure<UserModel>(result.Error);

        await _userRepository.CreateUserAsync(result.Value);

        return result.Value;
    }
}
