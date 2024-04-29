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
    
    public async Task CreateUserAsync(UserModel user)
    {
        _appDbContext.Add(user);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task<UserModel> FindByIdAsync(string id)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task DeleteUserAsync(UserModel userModel)
    {
        var user = await _appDbContext.Users.FindAsync(userModel.Id);
        if (user is null) return;

        _appDbContext.Remove(user);
        await _appDbContext.SaveChangesAsync();
    }
}