using Npgsql;
using Microsoft.EntityFrameworkCore;
using NuzlockeTrackerWeb.Components;
using NuzlockeTrackerWeb.Components.GameData;
using NuzlockeTrackerWeb.Data;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = builder.Environment.IsDevelopment() 
    ? $"http://localhost:{port}" 
    : $"http://0.0.0.0:{port}";
builder.WebHost.UseUrls(url);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
NpgsqlDataSource? dataSource = null;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        var connStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require,
            TrustServerCertificate = true,
            Pooling = true,
            CommandTimeout = 30 
        };

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStringBuilder.ToString());
        dataSourceBuilder.EnableDynamicJson(); 
        dataSource = dataSourceBuilder.Build();
        
        Console.WriteLine("NpgsqlDataSource with Dynamic JSON enabled.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"CRITICAL: DATABASE_URL parsing failed: {ex.Message}");
    }
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (dataSource != null)
    {
        // Use the DataSource instead of just the connection string
        options.UseNpgsql(dataSource);
    }
    else 
    {
        options.UseNpgsql("Data Source=localapp.db");    }
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<NuzlockeSessionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();
        
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") 
        {
            Console.WriteLine("Running EnsureCreated...");
            db.Database.Migrate();
            
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public';";
            db.Database.OpenConnection();
            using var reader = command.ExecuteReader();
            Console.WriteLine("Current Tables in Postgres:");
            while (reader.Read())
            {
                Console.WriteLine($"   - {reader[0]}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DATABASE STARTUP ERROR: {ex.Message}");
    }
}

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