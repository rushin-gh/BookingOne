using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Contracts;
using WebAPI.Models;
using WebAPI.Services;
using WebAPI.Database;

var builder = WebApplication.CreateBuilder(args);

// Adding key vault to configuration
var vaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(vaultUri))
{
    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ExcludeManagedIdentityCredential = true,   // the usual cause of the hang locally
        ExcludeInteractiveBrowserCredential = true
    });

    var client = new SecretClient(new Uri(vaultUri), credential, new SecretClientOptions
    {
        Retry = { MaxRetries = 2, NetworkTimeout = TimeSpan.FromSeconds(10) }
    });

    builder.Configuration.AddAzureKeyVault(client, new KeyVaultSecretManager());
}

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing configuration value: ConnectionStrings:DefaultConnection");
}

// Binding configuration to model
builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Authentication & Authorisation
var jwt = builder.Configuration.GetSection("JWT").Get<JwtOptions>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "sub",
        RoleClaimType = "role"
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
    .AddPolicy("OrganizerOnly", p => p.RequireRole("Organizer", "Admin"))
    .AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.MapSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
