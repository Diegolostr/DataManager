using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Hubs;
using UnityDataImporter.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Login";
        o.AccessDeniedPath = "/Login";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddRazorPages(o =>
    o.Conventions.AuthorizeFolder("/").AllowAnonymousToPage("/Login"));
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = long.MaxValue;
});
var connectionString = Environment.GetEnvironmentVariable("DB_CONN")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"[DB] Using connection: {connectionString?[..Math.Min(60, connectionString.Length)]}...");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<ItemRepository>();
builder.Services.AddScoped<WeaponDataRepository>();
builder.Services.AddScoped<MagicAttackRepository>();
builder.Services.AddScoped<SoundsArrayRepository>();
builder.Services.AddScoped<LootTableRepository>();
builder.Services.AddScoped<RecipeRepository>();
builder.Services.AddScoped<NpcShopRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var msg = System.Text.Json.JsonSerializer.Serialize(ex?.Error.Message);
        var inner = System.Text.Json.JsonSerializer.Serialize(ex?.Error.InnerException?.Message);
        await ctx.Response.WriteAsync($"{{\"error\":{msg},\"inner\":{inner}}}");
    }));
    app.UseHsts();
}


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers().AllowAnonymous();
app.MapRazorPages()
   .WithStaticAssets();

app.MapHub<DataHub>("/hub/data");

// API key login endpoint for Unity clients
app.MapPost("/api/auth/login", async (HttpContext ctx) =>
{
    var expectedKey = Environment.GetEnvironmentVariable("API_KEY");
    if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != expectedKey)
        return Results.Unauthorized();

    var claims = new[] { new System.Security.Claims.Claim("client", "unity") };
    var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Ok(new { message = "authenticated" });
}).AllowAnonymous();

app.Run();
