using DocProjDEVPLANT.Entities;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Repository;
using DocProjDEVPLANT.Repository.Repositories;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Services.Company;
using Microsoft.EntityFrameworkCore;

namespace DocProjDEVPLANT;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyService, CompanySerivce>();
       
        
        return services;
    }
}