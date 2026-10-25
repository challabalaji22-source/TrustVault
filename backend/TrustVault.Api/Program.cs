using Microsoft.AspNetCore.Identity;
using TrustVault.Api.Models;
using Microsoft.EntityFrameworkCore;
using TrustVault.Api.Data;
using TrustVault.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustVaultFrontend", policy =>
    {
        policy.WithOrigins(
    "http://localhost:4200",
    "http://localhost:55612"
)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<TrustVault.Api.Services.AuditService>();
builder.Services.AddSingleton<TrustVault.Api.Services.DocumentStorageService>();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TrustVaultDbContext>();
builder.Services.AddDbContext<TrustVaultDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("TrustVaultFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/db-test", async (TrustVaultDbContext db) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        var canConnect = true;

        return canConnect
            ? Results.Ok("PostgreSQL connection successful!")
            : Results.Problem("PostgreSQL connection returned false.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database test failed: {ex}");
        return Results.Problem("Database connection failed. Check the VS Code terminal.");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "TrustVault API is running!");
app.MapIdentityApi<ApplicationUser>();
app.MapDocumentEndpoints();
app.MapGet("/secure-test", (System.Security.Claims.ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        Message = "You are authenticated!",
        Email = user.Identity?.Name
    });
}).RequireAuthorization();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "DocumentOwner", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    var userManager = scope.ServiceProvider
    .GetRequiredService<UserManager<ApplicationUser>>();

var user = await userManager.FindByEmailAsync("balaji.test@example.com");

if (user != null && !await userManager.IsInRoleAsync(user, "DocumentOwner"))
{
    await userManager.AddToRoleAsync(user, "DocumentOwner");
}
}
app.Run();