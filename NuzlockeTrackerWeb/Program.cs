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

// --- 2. DATABASE REGISTRATION (ULTIMATE BULLETPROOF VERSION) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString = null;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        // Use the official builder to safely escape passwords and format the string
        var connStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = Npgsql.SslMode.Require,
            TrustServerCertificate = true,
            Pooling = true,
            CommandTimeout = 30 // Prevents crashing if DB is slow to wake up
        };

        connectionString = connStringBuilder.ToString();
        Console.WriteLine("✅ Database connection string built successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ CRITICAL: DATABASE_URL parsing failed: {ex.Message}");
    }
}
else
{
    Console.WriteLine("⚠️ DATABASE_URL environment variable is NULL or EMPTY.");
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
        Console.WriteLine("⚠️ No valid DATABASE_URL found. Using In-Memory Database.");
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
    try 
    {
        using var db = factory.CreateDbContext();
        
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
        {
            Console.WriteLine("🔄 Attempting to apply migrations...");
            db.Database.Migrate();
            Console.WriteLine("✅ Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        // This will print the exact reason the database is failing to connect in the Railway logs
        Console.WriteLine($"❌ DATABASE STARTUP ERROR: {ex.Message}");
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