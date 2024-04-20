using DocProjDEVPLANT.Controllers;
using DocProjDEVPLANT.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository;

public class UserRepository : Repository<IdentityUser> , IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        return await _appDbContext.Users.ToListAsync();
    }
}