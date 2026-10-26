using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using WebAPI.Models;

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

// Binding configuration to model
builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.MapSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
