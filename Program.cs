using GameShelfWeb.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configurar EF Core con SQLite en archivo local 'games.db'
var connectionString = $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "games.db")}";
builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));

// Registrar repositorio
builder.Services.AddScoped<GameRepository>();

var app = builder.Build();

// Seed: crear DB y datos si es necesario
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<GameRepository>();
    repo.SeedIfEmpty();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
