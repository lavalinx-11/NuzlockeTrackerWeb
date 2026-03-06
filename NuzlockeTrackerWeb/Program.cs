using Microsoft.EntityFrameworkCore;
using NuzlockeTrackerWeb.Components;
using NuzlockeTrackerWeb.Components.GameData;
using NuzlockeTrackerWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. PORT CONFIGURATION ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = builder.Environment.IsDevelopment() 
    ? $"http://localhost:{port}" 
    : $"http://0.0.0.0:{port}";
builder.WebHost.UseUrls(url);

// --- 2. DATABASE REGISTRATION (FIXED) ---
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

// We use AddDbContextFactory so Home.razor can @inject IDbContextFactory<AppDbContext>
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Production: Use PostgreSQL on Railway
        options.UseNpgsql(connectionString);
    }
    else 
    {
        // Local Testing: Use an In-Memory database
        options.UseInMemoryDatabase("NuzlockeLocalDB");
    }
});

// --- 3. APP SERVICES ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<NuzlockeSessionService>();

var app = builder.Build();

// --- 4. AUTO-CREATE DATABASE TABLES ---
using (var scope = app.Services.CreateScope())
{
    // Note: We use the factory even here to stay consistent
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
    {
        db.Database.Migrate();
    }
}

// --- 5. MIDDLEWARE & ROUTING ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();