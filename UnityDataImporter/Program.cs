using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("DATABASE_URL") ??
        builder.Configuration.GetConnectionString("DefaultConnection")));
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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
