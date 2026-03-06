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

// --- 2. DATABASE REGISTRATION (FIXED FOR RAILWAY) ---
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString = null;

if (!string.IsNullOrEmpty(rawConnectionString))
{
    // Railway provides "postgres://", but Npgsql driver often requires "postgresql://"
    connectionString = rawConnectionString.Replace("postgres://", "postgresql://");
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Production: Use PostgreSQL on Railway with the cleaned string
        options.UseNpgsql(connectionString);
    }
    else 
    {
        // Local Testing: Use an In-Memory database
        options.UseInMemoryDatabase("NuzlockeLocalDB");
        Console.WriteLine("⚠️ No DATABASE_URL found. Using In-Memory Database for local testing.");
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
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    
    // Only attempt migrations if we are connected to a real database (Postgres)
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
    {
        try 
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed: {ex.Message}");
            // We don't throw here so the app might still start, 
            // but you'll see the error in Railway logs.
        }
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