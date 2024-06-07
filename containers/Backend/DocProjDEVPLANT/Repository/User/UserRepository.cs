using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.User;

public class UserRepository :  IUserRepository
{
    protected readonly AppDbContext _appDbContext;
    
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
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

    public async Task<List<UserModel>> GetUsersByCompanyAsync(string companyName)
    {
        return await _appDbContext.Users
            .Where(u => u.Company.Name == companyName)
            .ToListAsync();
    }
    
    public async Task UpdateUserAsync(UserModel user)
    {
        _appDbContext.Users.Update(user); 
        await _appDbContext.SaveChangesAsync();
        
    }
}