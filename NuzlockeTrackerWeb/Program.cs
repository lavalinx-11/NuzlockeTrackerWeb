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

// --- 2. DATABASE REGISTRATION (BULLETPROOF VERSION) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString = null;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Manual Parsing of the Railway URI: postgres://user:password@host:port/database
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        connectionString = $"Host={uri.Host};" +
                           $"Port={uri.Port};" +
                           $"Username={userInfo[0]};" +
                           $"Password={userInfo[1]};" +
                           $"Database={uri.AbsolutePath.TrimStart('/')};" +
                           $"Pooling=true;" +
                           $"SSL Mode=Require;" +
                           $"Trust Server Certificate=true;";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
    }
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else 
    {
        options.UseInMemoryDatabase("NuzlockeLocalDB");
        Console.WriteLine("⚠️ No valid DATABASE_URL found or parsing failed. Using In-Memory Database.");
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
    
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
    {
        try 
        {
            // This builds the tables on Railway automatically
            db.Database.Migrate();
            Console.WriteLine("✅ Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed: {ex.Message}");
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