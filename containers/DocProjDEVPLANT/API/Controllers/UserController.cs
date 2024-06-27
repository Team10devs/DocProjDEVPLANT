using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Firebase;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;
[Route("/api/User")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IFirebaseService _firebaseService;
    private readonly ICompanyService _companyService;

    public UserController(IUserService _service,ITokenService tokenService,IEmailService emailService,
        IFirebaseService firebaseService,ICompanyService companyService)
    {
        _userService = _service;
        _tokenService = tokenService;
        _emailService = emailService;
        _firebaseService = firebaseService;
        _companyService = companyService;
    }

    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        Result<IEnumerable<UserModel>> result = await _userService.GetAllAsync();

        return Ok(result.Value.Select(Map));
    }

    [HttpGet("{email}")]
    public async Task<ActionResult<UserModel>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return Ok(Map(user));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser(
        [FromBody] UserRequest userRequest /*, [FromHeader] string authorization*/)
    {
        // if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        // {
        //     return Unauthorized("Authorization header is missing or invalid.");
        // }

        // string idToken = authorization.Substring("Bearer ".Length).Trim();

        // try
        // {
        //     var decodedToken = await _firebaseService.VerifyIdTokenAsync(idToken);
        //     string userEmail = decodedToken.Claims["email"].ToString();
        //     string userId = decodedToken.Uid;
        //string userEmail = "davidstana1@gmail.com";

        // var existingUser = await _userService.GetUserByEmailAsync(userRequest.email);

        // if (existingUser != null && existingUser.Role == RoleEnum.UnregisteredUser)
        // {
        //     //daca exista un utilizator neinregistrat
        //     existingUser.Role = RoleEnum.OrdinaryUser;
        //     
        //     await _userService.UpdateUserAsync(existingUser);
        //
        //     return Ok(existingUser);
        // }
        // else
        // {
        // daca nu exista , facem unul nou
    try
{
    CompanyModel company = null;

    if (!string.IsNullOrEmpty(userRequest.companyName))
    {
        company = await _companyService.GetCompanyByNameAsync(userRequest.companyName);

        if (company == null)
        {
            // companie noua si setare rol user la superUser
            try
            {
                var newCompanyRequest = new CompanyRequest(userRequest.companyName);
                var newCompany = await _companyService.CreateCompanyAsync(newCompanyRequest);
                if (newCompany == null)
                {
                    return BadRequest("Failed to create company.");
                }

                company = newCompany;
                
                userRequest = userRequest with { role = RoleEnum.SuperUser };
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create company: {ex.Message}");
            }
        }
        else
        {
            // daca exista compania userRole = ordinaryUser
            userRequest = userRequest with { role = RoleEnum.OrdinaryUser };
        }
    }

    var existingUser = await _userService.GetUserByEmailAsync(userRequest.email);

    if (existingUser == null)
    {
        // daca nu exista creem user
        var createdUser = await _userService.CreateUserAsync(userRequest);

        // adaugare in companie
        if (company != null)
        {
            var addUserToCompanyResult = await _companyService.AddUserToCompanyAsync(company.Id, createdUser.Id);
            if (addUserToCompanyResult.IsFailure)
            {
                return BadRequest("Failed to add user to company.");
            }
        }

        return Ok(Map(createdUser));
    }
    else if (existingUser.Role == RoleEnum.UnregisteredUser)
    {
        // actualizare user
        existingUser.UserName = userRequest.username;
        existingUser.Role = userRequest.role;

        await _userService.UpdateUserAsync(existingUser);

        // user actualizat in companie
        if (company != null)
        {
            var addUserToCompanyResult = await _companyService.AddUserToCompanyAsync(company.Id, existingUser.Id);
            if (addUserToCompanyResult.IsFailure)
            {
                return BadRequest("Failed to add user to company.");
            }
        }

        return Ok(Map(existingUser));
    }
    else
    {
        return BadRequest("User already exists and is not an UnregisteredUser.");
    }
}
catch (Exception e)
{
    return BadRequest(e.Message);
}


    

    // }
        // }
        // catch (UnauthorizedAccessException ex)
        // {
        //     return Unauthorized(ex.Message);
        // }
        // catch (Exception ex)
        // {
        //     return BadRequest("Failed to register");
        // }
    }

    [HttpPost("addIdVariables")]
    public async Task<IActionResult> AddIdVariables(IFormFile image)
    {
        var result = await _userService.AddIdVariables(image);
        if (result.IsSucces)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result.Error);
        }
    }
    
    [HttpGet("company/{companyName}")]
    public async Task<ActionResult<List<UserModel>>> GetUsersByCompany(string companyName)
    {
        var users = await _userService.GetUsersByCompanyAsync(companyName);
        if (users == null)
        {
            return NotFound();
        }
        return Ok(users);
    }
    
    [HttpPatch("updateUserPersonalData")]
    public async Task<ActionResult<UserModel>> UpdateUserPersonalData(string userId, [FromBody] UserPersonalData personalDataDto)
    {
        if (string.IsNullOrEmpty(userId) || personalDataDto == null)
        {
            return BadRequest("Invalid userId or personalData");
        }

        var result = await _userService.AddIdVariablesToUser(userId,personalDataDto);
        if (result.IsSucces)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }

    [HttpPatch("updateUserJsonData")]
    public async Task<ActionResult<UserResponse>> UpdateUserJsonData(string userEmail, string jsonData)
    {
        try
        {
            var user = await _userService.UpdateUserJson(userEmail, jsonData);
            return Ok(Map(user));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
     
    private UserResponse Map(UserModel userModel)
    {
        if (userModel.Company != null)
        {
            // Dacă utilizatorul are o companie asociată, poți adăuga și informații despre companie în răspuns
            return new UserResponse(
                userModel.Id,
                userModel.UserName,
                userModel.Role,
                userModel.UserData,
                userModel.Email,
                userModel.Company.Id// sau userModel.Company?.Id dacă Company poate fi null
            );
        }
        else
        {
            // Dacă utilizatorul nu are o companie asociată, poți crea răspunsul fără informații despre companie
            return new UserResponse(
                userModel.Id,
                userModel.UserName,
                userModel.Role,
                userModel.UserData,
                userModel.Email
            );
        }
    }

    
}