using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FlosonicsSession.Configurations;
using FlosonicsSession.DAL;
using FlosonicsSession.Data;
using FlosonicsSession.FluentValidations;
using FlosonicsSession.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var keyVaultConfig = builder.Configuration.GetSection("keyVaultConfig").Get<KeyVaultConfig>();


// Connects and reads Azure KeyVault  
var client = new SecretClient(new Uri(keyVaultConfig.KVUrl), 
    new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecretId));
builder.Configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());

// Add services to the container.

//DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration["FlosonicsSqlServerConnectionString"]));
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped<ISessionsRepository, InMemorySessionsRepository>();
builder.Services.AddSingleton(new SemaphoreSlim(1, 1));

builder.Services.AddScoped<ISessionValidator, SessionValidator>();
builder.Services.AddScoped<ISessionValidatorFactory, SessionValidatorFactory>();
//builder.Services.AddScoped<IValidator<ISessionDto>, SessionValidator>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Flosonics Medical", Version = "v1" });
    c.OperationFilter<AddHeaderParameters>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flosonics Medical");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();