using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
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
        await ctx.Response.WriteAsync($"{{\"error\":\"{ex?.Error.Message}\",\"stack\":\"{ex?.Error.StackTrace?.Replace("\"", "'")}\"}}");
    }));
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers().AllowAnonymous();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
