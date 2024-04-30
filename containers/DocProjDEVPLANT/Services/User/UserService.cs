using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;

    public UserService(IUserRepository repository, ICompanyRepository companyRepository)
    {
        _userRepository = repository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<IEnumerable<UserModel>>> GetAllAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<Result<UserModel>> GetByIdAsync(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);

        if (user is null)
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "User"));
        
        return user;
    } 
    public async Task<Result<UserModel>> CreateUserAsync(UserRequest request)
    {
        var company = await _companyRepository.FindByIdAsync(request.companyId);

        if (company is null)
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "Company"));
        
        var result = await UserModel.CreateAsync(
            _userRepository,
            company,
            request.username,
            request.email,
            request.address,
            request.fullname,
            request.cnp,
            request.role);
        
        if (result.IsFailure)
            return Result.Failure<UserModel>(result.Error);

        await _userRepository.CreateUserAsync(result.Value);

        return result.Value;
    }
}
