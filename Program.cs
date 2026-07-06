using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using DotNetWebApiMssql.Models.Context;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Models.EntityLogs;
using DotNetWebApiMssql.Interfaces;
using DotNetWebApiMssql.Settings;
using DotNetWebApiMssql.Services;
using DotNetWebApiMssql.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string connectionName = "DefaultConnection";
var connectionString = builder.Configuration.GetConnectionString(connectionName)
    ?? throw new InvalidOperationException($"Connection string '{connectionName}' is required.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<LdapSettings>(builder.Configuration.GetSection(LdapSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is required.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is required.");


if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    throw new InvalidOperationException("JwtSettings:Issuer is required.");


if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    throw new InvalidOperationException("JwtSettings:Audience is required.");

builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<TemplateContext>();

builder.Services.AddDbContext<TemplateContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
        options.EnableAnnotations();
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNetWebApiMssql", Version = "v1" });
        options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped(typeof(IRepositoryService<,>), typeof(RepositoryService<,>));
builder.Services.AddScoped<ILogicService, LogicService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(options =>
    {   
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            // 一般我們都會驗證 Audience
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        };
    });

builder.Services.AddAuthorization();

const string corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
