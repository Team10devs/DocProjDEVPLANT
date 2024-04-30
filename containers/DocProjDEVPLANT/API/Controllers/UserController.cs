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
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    [HttpGet("{Id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserModel>> CreateUser([FromBody] UserRequest userRequest)
    {
        var user = await _userService.CreateUserAsync(userRequest);

        if (user is null)
            return BadRequest();
        
        return Ok(user);
    }
    
    private UserResponse Map(UserModel userModel)
    {

        var company = new CompanyResponse(userModel.Company.Id, userModel.Company.Name);
        
        return new UserResponse(userModel.Id,
            userModel.UserName,
            userModel.FullName,
            userModel.CNP,
            userModel.Role,
            company.Id);
    }
}