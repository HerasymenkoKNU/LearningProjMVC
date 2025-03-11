using LearningDomain.Model;
using LearningInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<LearningMvcContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    
})
.AddEntityFrameworkStores<LearningMvcContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";       
    options.AccessDeniedPath = "/Account/AccessDenied"; 
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Teacher", "Student" };
    foreach (var roleName in roles)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
