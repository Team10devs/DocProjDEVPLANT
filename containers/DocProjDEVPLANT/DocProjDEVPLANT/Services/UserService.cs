using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services;

public class UserService
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
}