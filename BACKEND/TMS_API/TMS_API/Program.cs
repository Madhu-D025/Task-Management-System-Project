using AuthApplication.Controllers;
using AuthApplication.Services;
using LearningAPI.DBContext;
//using LearningAPI.DBContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WM.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthMasterServices>();
builder.Services.AddScoped<SMSService>();
builder.Services.AddScoped<AuthController>();
builder.Services.AddScoped<DocumentService>();
//builder.Services.AddScoped<ApprovalLogService>();
//builder.Services.AddScoped<StageService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(item => item.UseSqlServer(configuration.GetConnectionString("myconn")));
var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1073741824;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1073741824;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_073_741_824; // 1 GB
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

var staticFilePaths = new[] { "Documents", "ProfileAttachments" };
foreach (var path in staticFilePaths)
{
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);

    // Create the folder if it doesn't exist
    if (!Directory.Exists(fullPath))
    {
        Directory.CreateDirectory(fullPath);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), path)),
        RequestPath = $"/{path}",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }
    });
}

//Timer vendortimer = new Timer(backupdata, app, TimeSpan.Zero, TimeSpan.FromHours(4));

//Timer employeeattendancesumary = new Timer(updatecoursestatus, app, TimeSpan.Zero, TimeSpan.FromDays(1));
// Ensure default static files (e.g., wwwroot) are also CORS-enabled
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
});

app.UseRouting();

app.UseCors(policy => policy
     //.WithOrigins("http://localhost:3000")
     .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
//.AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();


//static async void backupdata(object state)
//{
//    var app = (WebApplication)state;

//    using (var scope = app.Services.CreateScope())
//    {
//        var serviceProvider = scope.ServiceProvider;


//        var backup = serviceProvider.GetRequiredService<AuthController>();


//        await backup.GenerateSqlScript();
//    }
//}
