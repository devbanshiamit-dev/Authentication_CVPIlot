using JWTAuthenticationAPI.JWT;
using JWTAuthenticationAPI.Middlewares;
using JWTAuthenticationAPI.Repository;
using JWTAuthenticationAPI.Services;
using JWTAuthenticationAPI.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<JWTService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var jwtService = new JWTService(builder.Configuration);

var app = builder.Build();

// Global exception handler - first in the pipeline.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
