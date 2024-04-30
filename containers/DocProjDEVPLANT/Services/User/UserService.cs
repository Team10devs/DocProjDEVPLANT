using DocProjDEVPLANT.API.User;
using DocProjDEVPLANT.Domain.Entities.User;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services.Utils.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Services.User;

public class UserService : IUserService
{
    //private readonly IUserRepository _userRepository;
    //private readonly ICompanyRepository _companyRepository;
    private readonly AppDbContext _context;

    public UserService(AppDbContext context,IUserRepository repository, ICompanyRepository companyRepository)
    {
        _context = context;
        //_userRepository = repository;
       // _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<UserModel>> GetAllAsync()
    {
        return await _context.Users
           // .Include(u => u.Company ) da ciclic
            .ToListAsync();
    }

    public async Task<UserModel> GetByIdAsync(string id)
    {
        var user = await _context.Users.Where(u => u.Id == id ).FirstOrDefaultAsync(); //da null useru 

        if (user is null)
            throw new NotImplementedException();
        
        return user;
    } 
    
    public async Task<UserModel> CreateUserAsync(UserRequest request)
    {
        var user = await UserModel.CreateAsync(request.username,
            request.email,
            request.address,
            request.fullname,
            request.cnp,
            request.role);

        if (user is null)
            throw new NotImplementedException();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}
