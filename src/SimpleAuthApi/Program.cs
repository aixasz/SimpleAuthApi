using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Entities;
using SimpleAuthApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Api"));

builder.Services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddUserStore<User>();

builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddTransient<IUserManagementService, UserManagementService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options
    .AddPolicy(
        "All",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("All");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapIdentityApi<User>();

app.Run();
