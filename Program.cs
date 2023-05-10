using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Application.Data;
using Application.Services;
using Application.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddRoles<IdentityRole>();

builder.Services.AddControllersWithViews();

// Register custom services
builder.Services.AddSingleton<IMessagingWebSocketService, MessagingWebSocketService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Main/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWebSockets();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Main}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Main}/{action=Index}/{id?}");
    endpoints.Map("/Messaging/WebSocket", async context =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var messagingWebSocketService = context.RequestServices.GetRequiredService<IMessagingWebSocketService>();
            await messagingWebSocketService.HandleWebSocketAsync(context, () => Task.CompletedTask);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    });
});


await ApplicationDbInitializer.SeedUsers(app);

await app.RunAsync();