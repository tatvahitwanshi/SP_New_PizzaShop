using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Repository;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzaShopApp.Helpers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<PizzaShopContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
builder.Services.AddScoped<ILogin, LoginRepository>();
builder.Services.AddSingleton<GenerateJwtTokenHelper>();
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<IUserProfile, UserProfileRepository>();
builder.Services.AddScoped<IUserList, UserListRepository>();
builder.Services.AddScoped<IRolesAndPermission, RolesAndPermissionRepository>();
builder.Services.AddScoped<IMenu, MenuRepository>();
builder.Services.AddScoped<ISectionTables, SectionTablesRepository>();
builder.Services.AddScoped<ITaxesFees, TaxesFeesRespository>();
builder.Services.AddScoped<IOrders, OrdersRepository>();
builder.Services.AddScoped<ICustomers, CustomersRepository>();
builder.Services.AddScoped<IKot, KotRepository>();
builder.Services.AddScoped<ITablesApp, TablesAppRepository>();
builder.Services.AddScoped<IWaitingList, WaitingListRepository>();
builder.Services.AddScoped<IMenuApp, MenuAppRepository>();
builder.Services.AddScoped<IDashboard, DashboardRepository>();
builder.Services.AddSingleton<GenerateJwtTokenHelper>();


// Configure JWT settings from appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register JWT Token Helper as a service
builder.Services.AddSingleton<GenerateJwtTokenHelper>();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache(); // Required for session state


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
