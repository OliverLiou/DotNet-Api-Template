using System.Text;
using  DotNetApiTemplate.Models;
using JwtAuthDemo.Helpers;
using  DotNetApiTemplate.Services;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var templateContext = "TemplateDBContext";
var connectionStr = builder.Configuration.GetConnectionString(templateContext);

builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<TemplateContext>();

builder.Services.AddDbContext<TemplateContext>(opt => opt.UseSqlServer(connectionStr));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 包含 XML 註解
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNetApiTemplate", Version = "v1" });
    // add JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {securityScheme, new string[] { }}
        });
});

// Add services to the container.
builder.Services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// 註冊泛型 Repository 服務
builder.Services.AddScoped(typeof(IRepositoryService<,>), typeof(RepositoryService<,>));

// 註冊具體實體的 Repository 服務
builder.Services.AddScoped<IRepositoryService<User, UserLog>, RepositoryService<User, UserLog>>();
builder.Services.AddScoped<IRepositoryService<Table1, Table1Log>, RepositoryService<Table1, Table1Log>>();

//JWT驗證加密
builder.Services.AddScoped<JwtHelpers>();
builder.Services.AddAuthentication((cfg =>
{
    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}))
.AddJwtBearer(options =>
{
    // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
    options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

        // 一般我們都會驗證 Issuer
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

        // 通常不太需要驗證 Audience
        ValidateAudience = false,
        //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

        // 一般我們都會驗證 Token 的有效期間
        ValidateLifetime = true,

        // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
        ValidateIssuerSigningKey = false,

        // "1234567890123456" 應該從 IConfiguration 取得
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
    };
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {

// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:5001", "http://localhost:3000")

            .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
