using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Identity.Api.Configuration;
using Identity.Api.Middleware;
using Identity.Api.Serialization;
using Identity.Application.Abstractions;
using Identity.Application.Common;
using Identity.Application.Errors;
using Identity.Application.Services;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

EnvFileLoader.LoadFromNearest(
    Directory.GetCurrentDirectory(),
    AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);
var isEfDesignTime = IsEfDesignTime();

var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection is required. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");

var jwtOptions = ResolveJwtOptions(builder.Configuration);
if (!isEfDesignTime)
{
    ValidateJwtOptions(jwtOptions);
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = jwtOptions.Secret;
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.ExpiresMinutes = jwtOptions.ExpiresMinutes;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options => ApiJsonOptions.Configure(options.JsonSerializerOptions));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => ToCamelCase(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid value"
                        : error.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(ApiResponse<object>.Fail(new ApiError(
            ErrorCodes.ValidationError,
            "Validation failed",
            details)));
    };
});

builder.Services.ConfigureHttpJsonOptions(options => ApiJsonOptions.Configure(options.SerializerOptions));

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IdentityDatabaseSeeder>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                string.IsNullOrWhiteSpace(jwtOptions.Secret)
                    ? "design-time-only-placeholder-secret"
                    : jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return WriteErrorAsync(
                    context.Response,
                    StatusCodes.Status401Unauthorized,
                    ErrorCodes.Unauthorized,
                    "Unauthorized");
            },
            OnForbidden = context => WriteErrorAsync(
                context.Response,
                StatusCodes.Status403Forbidden,
                ErrorCodes.Forbidden,
                "Forbidden")
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = ResolveCorsOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Automated Grading System Identity Service",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token. Example: Bearer {token}",
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

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.HasStarted)
    {
        return;
    }

    var (code, message) = response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => (ErrorCodes.Unauthorized, "Unauthorized"),
        StatusCodes.Status403Forbidden => (ErrorCodes.Forbidden, "Forbidden"),
        StatusCodes.Status404NotFound => (ErrorCodes.UserNotFound, "Resource not found"),
        _ => (ErrorCodes.InternalError, "Request failed")
    };

    await WriteErrorAsync(response, response.StatusCode, code, message);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDatabaseSeeder>();
    await seeder.SeedDevelopmentAccountsAsync();
}

app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

static JwtOptions ResolveJwtOptions(IConfiguration configuration)
{
    return new JwtOptions
    {
        Secret = configuration["JWT_SECRET"] ?? configuration["Jwt:Secret"] ?? string.Empty,
        Issuer = configuration["JWT_ISSUER"] ?? configuration["Jwt:Issuer"] ?? "ags",
        Audience = configuration["JWT_AUDIENCE"] ?? configuration["Jwt:Audience"] ?? "ags-api",
        ExpiresMinutes = ResolveExpiresMinutes(configuration)
    };
}

static int ResolveExpiresMinutes(IConfiguration configuration)
{
    var configuredValue = configuration["JWT_EXPIRES_MINUTES"] ?? configuration["Jwt:ExpiresMinutes"];
    return int.TryParse(configuredValue, out var expiresMinutes) && expiresMinutes > 0
        ? expiresMinutes
        : 1440;
}

static void ValidateJwtOptions(JwtOptions options)
{
    if (string.IsNullOrWhiteSpace(options.Secret))
    {
        throw new InvalidOperationException("JWT_SECRET is required.");
    }

    if (Encoding.UTF8.GetByteCount(options.Secret) < 32)
    {
        throw new InvalidOperationException("JWT_SECRET must be at least 32 bytes for HMAC signing.");
    }
}

static bool IsEfDesignTime()
{
    var commandLine = Environment.CommandLine;

    return commandLine.Contains("ef.dll", StringComparison.OrdinalIgnoreCase)
        || commandLine.Contains("dotnet-ef", StringComparison.OrdinalIgnoreCase)
        || AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            string.Equals(
                assembly.GetName().Name,
                "Microsoft.EntityFrameworkCore.Design",
                StringComparison.Ordinal));
}

static string[] ResolveCorsOrigins(IConfiguration configuration)
{
    var configuredOrigins = configuration["CORS_ALLOWED_ORIGINS"];

    if (!string.IsNullOrWhiteSpace(configuredOrigins))
    {
        return configuredOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(origin => origin.Trim().Trim('[', ']'))
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray();
    }

    var appsettingsOrigins = configuration
        .GetSection("Cors:AllowedOrigins")
        .GetChildren()
        .Select(section => section.Value)
        .OfType<string>()
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .ToArray();

    return appsettingsOrigins.Length > 0
        ? appsettingsOrigins
        : ["http://localhost:3000", "http://localhost:5173"];
}

static Task WriteErrorAsync(HttpResponse response, int statusCode, string code, string message)
{
    response.StatusCode = statusCode;
    response.ContentType = "application/json";

    var envelope = ApiResponse<object>.Fail(new ApiError(code, message));
    return response.WriteAsJsonAsync(envelope, ApiJsonOptions.SerializerOptions);
}

static string ToCamelCase(string key)
{
    if (string.IsNullOrWhiteSpace(key))
    {
        return key;
    }

    var lastSegment = key.Split('.').Last();
    return char.ToLowerInvariant(lastSegment[0]) + lastSegment[1..];
}
