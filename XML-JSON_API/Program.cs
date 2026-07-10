using Microsoft.AspNetCore.Authentication;
using XML_JSON_API.Auth;
using XML_JSON_API.Models;
using XML_JSON_API.Repositories;
using XML_JSON_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Bind BasicAuth section from appsettings.json
builder.Services.Configure<BasicAuthOptions>(
    builder.Configuration.GetSection(BasicAuthOptions.SectionName));

// Add authentication using our custom Basic scheme
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        "BasicAuthentication", null);

builder.Services.AddAuthorization();

//Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services
builder.Services.AddSingleton<FileProcessingService>();

//Repositories
builder.Services.AddSingleton<MemoryRepository>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();   // <-- must come BEFORE UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();