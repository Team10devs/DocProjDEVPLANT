using System.Dynamic;
using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Scanner;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

namespace DocProjDEVPLANT.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOcrService _ocrService;

    public UserService(IUserRepository repository,IOcrService ocrService)
    {
        _userRepository = repository;
        _ocrService = ocrService;
    }

    public async Task<Result<IEnumerable<UserModel>>> GetAllAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<UserModel> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _userRepository.FindByEmailAsync(email);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<Result<UserModel>> GetByIdAsync(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);

        if (user is null)
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "User"));
        
        return user;
    } 
    public async Task<UserModel> CreateUserAsync(UserRequest request)
    {
        //var company = await _companyRepository.FindByIdAsync(request.companyId);

      //  if (company is null)
        //    return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "Company"));

        
            var isEmailUnique = _userRepository.IsEmailUnique(request.email);

            if ( isEmailUnique.Result )
            {

                var result = await UserModel.CreateAsync(
                    request.email,
                    request.username,
                    request.role);

                if (result.IsFailure)
                    throw new Exception(result.Error.ToString());

                await _userRepository.CreateUserAsync(result.Value);

                return result.Value;
            }
            
            throw new Exception($"An user is already registered with email: {request.email}");

    }
    
    public async Task<List<UserModel>> GetUsersByCompanyAsync(string companyName)
    {
        return await _userRepository.GetUsersByCompanyAsync(companyName);
    }
    
    public async Task<Result> AddIdVariables(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return Result.Failure(new Error(ErrorType.NotFound, "Invalid image file"));
        }
        
        var tempPath = Path.GetTempFileName();
        
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }
        
        var ocrText = _ocrService.ExtractTextFromImage(tempPath);
        var mrzData = _ocrService.ExtractMrzData(ocrText);
        
        if (mrzData == null)
        {
            return Result.Failure(new Error(ErrorType.None, "Failed to extract MRZ data from the image."));
        }

        return Result.Succes(mrzData);
    }

    public async Task<Result<UserModel>> AddIdVariablesToUser(string userId, UserPersonalData personalDataDto)
    {
        var user = await _userRepository.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure<UserModel>(new Error(ErrorType.NotFound, "User not found"));
        }
        
        dynamic userData = JsonConvert.DeserializeObject<dynamic>(user.UserData) ?? new ExpandoObject();
        userData.client = userData.client ?? new ExpandoObject();
        
        userData.client.nume = personalDataDto.Nume;
        userData.client.cetatenie = personalDataDto.Cetatenie;
        userData.client.cnp = personalDataDto.CNP;
        userData.client.localitate = personalDataDto.Judet;
        userData.client.adresa = personalDataDto.Address;
        userData.client.tara = personalDataDto.Country;

        user.UserData = JsonConvert.SerializeObject(userData);
        
        /*var userDataJson = JsonConvert.SerializeObject(new { doc = personalDataDto }); 
        user.UserData = userDataJson; */
        //cred ca e useless asta 

        await _userRepository.UpdateUserAsync(user);

        return Result.Succes(user);
    }

    public async Task UpdateUserAsync(UserModel user)
    {
        await _userRepository.UpdateUserAsync(user);
    }

    public async Task<CompanyModel?> GetCompanyByUserEmail(string email)
    {
        try
        {
            var user = await _userRepository.FindByEmailAsync(email);
            return user.Company;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
