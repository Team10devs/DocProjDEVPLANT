using System.Configuration;
using DocProjDEVPLANT.Repository;
using DocProjDEVPLANT.Repository.Company;
using DocProjDEVPLANT.Repository.Database;
using DocProjDEVPLANT.Repository.User;
using DocProjDEVPLANT.Services;
using DocProjDEVPLANT.Services.Company;
using DocProjDEVPLANT.Services.Firebase;
using DocProjDEVPLANT.Services.InviteLinkToken;
using DocProjDEVPLANT.Services.Mail;
using DocProjDEVPLANT.Services.Minio;
using DocProjDEVPLANT.Services.Scanner;
using DocProjDEVPLANT.Services.Template;
using DocProjDEVPLANT.Services.User;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Newtonsoft.Json;

namespace DocProjDEVPLANT;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        string firebaseJsonPath = Path.Combine(AppContext.BaseDirectory, "firebase.json");
        var credential = GoogleCredential.FromFile(firebaseJsonPath);
        var firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            Credential = credential
        });
        services.AddSingleton(firebaseApp);
        
        services.AddScoped<IFirebaseService, FirebaseService>();
        
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