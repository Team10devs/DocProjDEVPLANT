using DocProjDEVPLANT.Repository;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Scanner;
using DocProjDEVPLANT.Services.Template;
using DocProjDEVPLANT.Services.User;
using Microsoft.EntityFrameworkCore;
using Minio;

namespace DocProjDEVPLANT;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });
        
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ITemplateService, TemplateService>();
        
        services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IMinioService, MinioService>();
        services.AddScoped<MinioClient>();
        
        services.AddScoped<ITokenService, TokenService>();
        
        return services;
    }
}