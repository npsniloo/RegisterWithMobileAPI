using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RegisterApp.Auth;
using RegisterApp.DB;
using RegisterApp.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IPhoneNormalizer, IRANPhoneNormalizer>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ISmsSender, ConsoleSmsSender>(); // swap to Twilio in prod
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(2),
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT authentication failed: {Error}", context.Exception.Message);
            if (context.Exception is SecurityTokenExpiredException)
                logger.LogError("Token has expired");
            else if (context.Exception is SecurityTokenInvalidSignatureException)
                logger.LogError("Token signature is invalid");
            else if (context.Exception is SecurityTokenInvalidAudienceException)
                logger.LogError("Token audience is invalid");
            else if (context.Exception is SecurityTokenInvalidIssuerException)
                logger.LogError("Token issuer is invalid");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT token validated successfully for user: {UserId}", 
                context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            logger.LogWarning("JWT challenge triggered. Error: {Error}, ErrorDescription: {Description}, AuthHeader: {AuthHeader}", 
                context.Error, context.ErrorDescription, string.IsNullOrEmpty(authHeader) ? "(missing)" : authHeader.Substring(0, Math.Min(20, authHeader.Length)) + "...");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            logger.LogInformation("JWT message received. AuthHeader present: {HasHeader}, Value: {Value}", 
                !string.IsNullOrEmpty(authHeader), string.IsNullOrEmpty(authHeader) ? "(none)" : authHeader.Substring(0, Math.Min(30, authHeader.Length)) + "...");
            return Task.CompletedTask;
        }
    };
});


// Simple rate limits (per-IP) to protect OTP endpoints
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("otp", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    });
});


builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "RegisterApp API", 
        Version = "v1",
        Description = "API for user registration and authentication"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token below (without 'Bearer' prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RegisterApp API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at /
    });
}
app.UseSwagger();




app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
