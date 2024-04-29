using DocProjDEVPLANT.Entities;
using DocProjDEVPLANT.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<UserModel> Users { get; set; } 
    public DbSet<CompanyModel> Companies { get; set; }
}