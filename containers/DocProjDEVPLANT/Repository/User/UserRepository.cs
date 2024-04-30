using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.User;

public class UserRepository : Repository<UserModel> , IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        return await _appDbContext.Users
            .Include(u => u.Company)
            .ToListAsync();
    }
    
    public async Task CreateUserAsync(UserModel userModel)
    {
        _appDbContext.Add(userModel);
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