using System.Text;
using BijliSaver.Api.Data;
using BijliSaver.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Render (and most other hosts) inject PORT and expect the app to bind to
// it; fall back to 8080 for local/Docker runs where it isn't set.
builder.WebHost.UseUrls($"http://+:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

// Managed Postgres providers (Neon, etc.) hand out a postgres:// URI, which
// Npgsql's connection-string parser doesn't accept directly — convert it if
// present. Falls back to ConnectionStrings:Postgres (appsettings / user
// secrets) locally.
var postgresConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") is { } databaseUrl
    ? NpgsqlConnectionStringFromUri(databaseUrl)
    : builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(postgresConnectionString));

builder.Services.AddHttpClient<OcrClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["OcrService:BaseUrl"]!);
    c.Timeout = TimeSpan.FromSeconds(90);
});

builder.Services.AddScoped<InsightsService>();
builder.Services.AddScoped<TokenService>();

// ---- JWT authentication ----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Render's free tier has no private networking, so the frontend calls this
// API over the public internet — allowed origins come from config (set via
// the AllowedOrigins__0, AllowedOrigins__1, ... env vars in production) with
// the local Vite dev servers as a fallback.
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    is { Length: > 0 } configured
        ? configured
        : ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string NpgsqlConnectionStringFromUri(string uri)
{
    var parsed = new Uri(uri);
    var userInfo = parsed.UserInfo.Split(':', 2);
    return new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = parsed.Host,
        Port = parsed.Port,
        Database = parsed.AbsolutePath.TrimStart('/'),
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        SslMode = Npgsql.SslMode.Require,
    }.ToString();
}
