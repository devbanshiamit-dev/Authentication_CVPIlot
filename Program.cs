using JWTAuthenticationAPI.UserService;
using JWTAuthenticationAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using JWTAuthenticationAPI.Repository;
using JWTAuthenticationAPI.Services;
using JWTAuthenticationAPI.JWT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<JWTService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new JWTService(builder.Configuration)
                .CreateValidationParameters();
    });

builder.Services.AddAuthorization();

var jwtService = new JWTService(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
