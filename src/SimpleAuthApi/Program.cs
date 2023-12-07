using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleAuthApi.Configuration;
using SimpleAuthApi.Domain;
using SimpleAuthApi.Domain.Entities;
using SimpleAuthApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Api"));

builder.Services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

var jwtSettingsConfiguration = builder.Configuration.GetSection(nameof(JwtSettings));
var jwtSettings = jwtSettingsConfiguration.Get<JwtSettings>();
builder.Services.AddOptions<JwtSettings>()
    .Bind(jwtSettingsConfiguration)
    .ValidateDataAnnotations();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddTransient<IUserManagementService, UserManagementService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();

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

SeedTestData();

void SeedTestData()
{
    using var scope = app.Services.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var userManager = scopedServices.GetRequiredService<UserManager<User>>();

    var users = new[]
    {
        new User
        {
            Id = Guid.Parse("5a8efb83-1530-4cfd-aad0-c61b40fb5858"),
            FirstName = "Thong",
            LastName = "Smith",
            Email = "thong.smith@test.com",
            UserName = "thong.smith@test.com"
        },
        new User
        {
            Id = Guid.Parse("598d4799-3f99-40a2-8bc1-f949f1ce911d"),
            FirstName = "Sai",
            LastName = "Tama",
            Email = "saitama@onepunch.man",
            UserName = "saitama@onepunch.man"
        },
        new User
        {
            Id = Guid.Parse("361940b3-d39e-440d-b1bf-70f9151fc3bf"),
            FirstName = "Thai",
            LastName = "Smile",
            Email = "thai@smile.com",
            UserName = "thai@smile.com"
        },
        new User
        {
            Id = Guid.Parse("7231653b-543f-44ed-b6d6-02c4d5e1326a"),
            FirstName = "For",
            LastName = "Delete",
            Email = "for@delete.com",
            UserName = "for@delete.com"
        },
    };

    foreach (var user in users)
    {
        var result = userManager.CreateAsync(user, "P@55w0rd!").Result;
    }
}

await app.RunAsync();