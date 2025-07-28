using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using test_2.Models;
using test_2.Services;

var builder = WebApplication.CreateBuilder(args);

// Cho phép sử dụng HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // Add HttpClient for PaymentController
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); // Add PaymentService
builder.Services.AddScoped<PayOSService>(); // Add PayOSService
builder.Services.AddScoped<EmailTestService>();
builder.Services.AddScoped<IVoucherService, VoucherService>(); // Add VoucherService
builder.Services.AddScoped<IInvoiceService, InvoiceService>(); // Add InvoiceService
// Kết nối DB
var connectionString = builder.Configuration.GetConnectionString("MyGarageFinalConnection");
builder.Services.AddDbContext<MyGarageFinalContext>(options =>
    options.UseSqlServer(connectionString));

// Cấu hình Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("GoogleKeys:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("GoogleKeys:ClientSecret");
});

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký MVC
builder.Services.AddControllersWithViews();
// Đăng ký SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, test_2.Services.CustomUserIdProvider>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Middleware log lỗi toàn cục
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("[GLOBAL ERROR] " + ex);
        throw;
    }
});

// Route mặc định (Controller-based routing)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ RẤT QUAN TRỌNG: Cho phép sử dụng [Route(...)]
app.MapControllers();
app.MapHub<test_2.Services.ChatHub>("/chathub");

app.Run();
