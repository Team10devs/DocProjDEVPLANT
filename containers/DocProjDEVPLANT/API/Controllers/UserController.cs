using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Utils.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace DocProjDEVPLANT.Controllers;
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

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser([FromBody] UserRequest user)
    {

        var result = await _userService.CreateUserAsync(user);

        if (result.IsSucces)
        {
            return Ok(result.Value); 
        }
        else
        {
            return BadRequest(result.Error);
        }
    }
}