
using DocProjDEVPLANT;
using DocProjDEVPLANT.Repository;
using DocProjDEVPLANT.Services.InviteLinkToken;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

services.AddCors(options => options.AddPolicy("EnableAll", policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
    policy.AllowAnyHeader();
}));

services.AddControllers();

// Add Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Add repositories
services.AddRepositories(configuration);

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("EnableAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();