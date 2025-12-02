using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using step_up.Models;
using step_up.Services;


var builder = WebApplication.CreateBuilder(args);



// Добавление DbContext и Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление Identity
builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
      .AddDefaultUI();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14); // или сколько тебе нужно
});


//builder.Services.AddScoped<SignInManager<User>>();

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<AttendanceBackgroundService>();

//builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Инициализация данных (создание администратора)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedData.Initialize(services, userManager, roleManager);
}

// HTTP pipeline
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
    name: "instructors",
    pattern: "Instructors/{action=Index}/{id?}",
    defaults: new { controller = "Instructors" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();

//public static class SeedData
//{
//    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
//    {
//        var adminRole = "Admin";
//        var userRole = "User";

//        // Проверяем, существует ли роль администратора, если нет - создаем её
//        var roleExist = await roleManager.RoleExistsAsync(adminRole);
//        if (!roleExist)
//        {
//            await roleManager.CreateAsync(new IdentityRole(adminRole));
//        }

//        var adminEmail = "ju00sk@gmail.com";
//        var adminPassword = "Yulia_01";

//        var user = await userManager.FindByEmailAsync(adminEmail);

//        if (user == null)
//        {
//            user = new User
//            {
//                UserName = adminEmail,
//                Email = adminEmail,
//                FullName = "Admin User",
//                CardNumber = "10000" // Устанавливаем значение для CardNumber
//            };

//            // Создаем пользователя
//            var result = await userManager.CreateAsync(user, adminPassword);
//            if (result.Succeeded)
//            {
//                // После создания пользователя, добавляем роль администратора
//                await userManager.AddToRoleAsync(user, adminRole);

//                // Также можно принудительно обновить пользователя, чтобы изменения были сохранены.
//                await userManager.UpdateAsync(user);
//            }
//        }
//        else
//        {
//            // Если пользователь существует, проверяем, есть ли у него роль "Admin".
//            var roles = await userManager.GetRolesAsync(user);
//            if (!roles.Contains(adminRole))
//            {
//                await userManager.AddToRoleAsync(user, adminRole);
//                await userManager.UpdateAsync(user); // Обновляем пользователя, чтобы изменения вступили в силу
//            }
//        }
//    }
//}

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var adminRole = "Admin";
        var userRole = "User";

        // Создание ролей, если их нет
        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        if (!await roleManager.RoleExistsAsync(userRole))
            await roleManager.CreateAsync(new IdentityRole(userRole));

        // Создание администратора
        var adminEmail = "ju00sk@gmail.com";
        var adminPassword = "Yulia_01";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Admin User",
                CardNumber = "10000"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
        else
        {
            var roles = await userManager.GetRolesAsync(adminUser);
            if (!roles.Contains(adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }

        // Создание 15 тестовых клиентов
        for (int i = 1; i <= 15; i++)
        {
            var email = $"testuser{i:00}@gmail.com";
            var password = $"TestUser{i:00}!";
            var fullName = $"Test User {i}";
            var cardNumber = (10001 + i).ToString();

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    CardNumber = cardNumber
                };

                var createResult = await userManager.CreateAsync(newUser, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, userRole);
                }
            }
        }
    }
}