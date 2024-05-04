using DocProjDEVPLANT.Repository;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Template;
using DocProjDEVPLANT.Services.User;
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

        services.AddScoped<ITemplateService, TemplateService>();
        
        return services;
    }
}