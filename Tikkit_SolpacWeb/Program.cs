using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tikkit_SolpacWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Tikkit_SolpacWeb.Services.Email;
using OfficeOpenXml;
using Tikkit_SolpacWeb.Hubs;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Tikkit_SolpacWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Tikkit_SolpacWebContext") ?? throw new InvalidOperationException("Connection string 'Tikkit_SolpacWebContext' not found.")));

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

builder.Services.AddSignalR();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffAndClientOnly", policy => policy.RequireRole("Staff", "Client"));
});
// Add services to the container.
builder.Services.AddControllersWithViews();
    
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie();

builder.Services.AddTransient<EmailSender>();


var app = builder.Build();


var supportedCultures = new[]
{
    new CultureInfo("vi-VN"),
    new CultureInfo("en-US"),
    new CultureInfo("ja-JP")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHub<NotificationHub>("/notificationHub");



app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}/{id?}");

app.Run();
