using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.Enums;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.Firebase;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;
[Route("User")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IFirebaseService _firebaseService;

    public UserController(IUserService _service,ITokenService tokenService,IEmailService emailService,
        IFirebaseService firebaseService)
    {
        _userService = _service;
        _tokenService = tokenService;
        _emailService = emailService;
        _firebaseService = firebaseService;
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
            return Ok(user);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser([FromBody] UserRequest userRequest/*, [FromHeader] string authorization*/)
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
                    var user = await _userService.CreateUserAsync(userRequest);
                    return Ok(Map(user));
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

    [HttpPatch("ChangeEmailState")]
    public async Task<ActionResult<UserModel>> ChangeIsEmail(string userId)
    {
        var result = await _userService.ChangeIsEmailToTrue(userId);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
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
     
    private UserResponse Map(UserModel userModel)
    {
        if (userModel.Company != null)
        {
            // Dacă utilizatorul are o companie asociată, poți adăuga și informații despre companie în răspuns
            return new UserResponse(
                userModel.Id,
                userModel.UserName,
                userModel.FullName,
                userModel.CNP,
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
                userModel.FullName,
                userModel.CNP,
                userModel.Role,
                userModel.UserData,
                userModel.Email
            );
        }
    }

    
}