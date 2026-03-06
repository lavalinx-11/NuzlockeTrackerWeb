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

// --- 2. DATABASE REGISTRATION ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string? connectionString = null;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

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
            CommandTimeout = 30 
        };

        connectionString = connStringBuilder.ToString();
        Console.WriteLine("✅ Database connection string built successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ CRITICAL: DATABASE_URL parsing failed: {ex.Message}");
    }
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        // Standard Npgsql connection without naming plugins
        options.UseNpgsql(connectionString);
    }
    else 
    {
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
    var services = scope.ServiceProvider;
    try 
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();
        
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
        {
            Console.WriteLine("🔄 Running EnsureCreated...");
            // This builds the database schema based on your AppDbContext settings
            db.Database.EnsureCreated();
            
            // LOGGING: This will tell us if Postgres made it "Matches" or "matches"
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public';";
            db.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            Console.WriteLine("📂 Current Tables in Postgres:");
            while (reader.Read())
            {
                Console.WriteLine($"   - {reader[0]}");
            }
        }
    }
    catch (Exception ex)
    {
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