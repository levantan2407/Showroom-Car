using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Data;
using CarShowroom.Models;

// Tu dong nhan dien dung thu muc ContentRoot va wwwroot du chay bang dotnet run hoac click truc tiep file .exe trong bin\Debug
var contentRoot = Directory.GetCurrentDirectory();
var webRoot = Path.Combine(contentRoot, "wwwroot");

if (!Directory.Exists(webRoot))
{
    var baseDir = AppContext.BaseDirectory;
    var binWwwRoot = Path.Combine(baseDir, "wwwroot");
    var projectWwwRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "wwwroot"));
    var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

    if (Directory.Exists(binWwwRoot))
    {
        contentRoot = baseDir;
        webRoot = binWwwRoot;
    }
    else if (Directory.Exists(projectWwwRoot))
    {
        contentRoot = projectRoot;
        webRoot = projectWwwRoot;
    }
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Directory.Exists(webRoot) ? webRoot : null
});

// Neu khong co port tu bien moi truong, mac dinh lang nghe tai http://localhost:5201
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://localhost:5201");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configure SecurityStampValidator interval for immediate lockout enforcement
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Seed database on startup
try
{
    using var scope = app.Services.CreateScope();
    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo và seed cơ sở dữ liệu.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
