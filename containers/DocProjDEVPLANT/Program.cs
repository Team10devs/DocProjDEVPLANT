using DocProjDEVPLANT;
using DocProjDEVPLANT.Entities.User;
using DocProjDEVPLANT.Repository;
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

<<<<<<< HEAD:containers/DocProjDEVPLANT/DocProjDEVPLANT/Program.cs
app.UseCors("EnableAll");
// app.UseHttpsRedirection();


=======
app.UseHttpsRedirection();
app.UseCors("EnableAll");
>>>>>>> c9de7be47c84bf676995ec419ca0a2cb066af4e9:containers/DocProjDEVPLANT/Program.cs
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();