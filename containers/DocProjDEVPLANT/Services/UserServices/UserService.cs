using DocProjDEVPLANT.Controllers;
using DocProjDEVPLANT.Entities.Company;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Utils.ResultPattern;

namespace DocProjDEVPLANT.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public readonly ICompanyRepository _companyRepository;

    public UserService(IUserRepository repository, ICompanyRepository companyRepository)
    {
        _userRepository = repository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<IEnumerable<UserModel>>> GetAllAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<Result<UserModel>> CreateUserAsync(UserRequest request)
    {
        var company = await _companyRepository.FindById(request.companyId);

        if (company is null)
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "Company not found ! ")); 

        var result = await UserModel.CreateAsync(
            _userRepository,
            request.username,
            request.password,
            request.email,
            request.role);
        
        if (result.IsFailure)
            return Result.Failure<UserModel>(result.Error);

        await _userRepository.CreateUserAsync(result.Value);

        return result.Value;
    }
}
