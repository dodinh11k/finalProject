using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using test_2.Models; // Thay đổi namespace nếu khác

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
// Lấy connection string từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MyGarageFinalConnection");

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<MyGarageFinalContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Authentication Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";         // Trang login mặc định
        options.LogoutPath = "/Account/Logout";       // Trang logout
        options.AccessDeniedPath = "/Account/Login";  // Trường hợp bị deny truy cập
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Thời gian cookie hết hạn
        options.SlidingExpiration = true;             // Tự động kéo dài thời gian nếu có hoạt động
    });

// Đăng ký Session (sử dụng bộ nhớ trong, mặc định)
builder.Services.AddDistributedMemoryCache(); // Bắt buộc để dùng Session

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian timeout session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký MVC (Controller + Views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware xử lý request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự Middleware rất quan trọng
app.UseSession();           // Kích hoạt Session
app.UseAuthentication();    // Kích hoạt xác thực (Cookie Auth)
app.UseAuthorization();     // Kích hoạt phân quyền

// Map route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
