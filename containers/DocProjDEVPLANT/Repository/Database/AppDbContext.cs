using DocProjDEVPLANT.Domain.Entities.Company;
using DocProjDEVPLANT.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<UserModel> Users { get; set; } 
    public DbSet<CompanyModel> Companies { get; set; }
}