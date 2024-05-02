using DocProjDEVPLANT.API.Company;
using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Services.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.API.Controllers;
[Route("User")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService _service)
    {
        _userService = _service;
    }

    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        Result<IEnumerable<UserModel>> result = await _userService.GetAllAsync();

        return Ok(result.Value.Select(Map));
    }

    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser([FromBody] UserRequest userRequest)
    {
        var result = await _userService.CreateUserAsync(userRequest);

        if (result.IsSucces)
        {
            return Ok(Map(result.Value)); 
        }
        else
        {
            return BadRequest(result.Error);
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
                userModel.FullName,
                userModel.CNP,
                userModel.Role,
                userModel.Company.Id // sau userModel.Company?.Id dacă Company poate fi null
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
                userModel.Role
            );
        }
    }

}