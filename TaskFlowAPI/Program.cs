using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskFlowAPI.src.entity;
using TaskFlowAPI.src.entity.board.services;
using TaskFlowAPI.src.entity.task.services;
using TaskFlowAPI.src.entity.user.services;
using TaskFlowAPI.src.common.options;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName.ToLower();
Console.WriteLine($"Ambiente: {environment}");

var aspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "";
var isContainer = aspNetUrls.Contains("+:80") || aspNetUrls.Contains("0.0.0.0:80");

if (!isContainer)
{
    var envFile = environment == "production" ? ".env.prd" : ".env.dev";
    var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
    var envPath = Path.Combine(projectRoot, envFile);

    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        Console.WriteLine($"{envFile} carregado");
    }
    else
    {
        Console.WriteLine($"{envFile} não encontrado");
    }
}
else
{
    Console.WriteLine($"Usando variáveis do container");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();


var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT");

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};";
}

Console.WriteLine($"Database: {connectionString.Split(';').FirstOrDefault(x => x.StartsWith("Database="))}");
Console.WriteLine($"Host: {connectionString.Split(';').FirstOrDefault(x => x.StartsWith("Host="))}");
Console.WriteLine(connectionString);

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();

string JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET");
string JWT_ISSUER = Environment.GetEnvironmentVariable("JWT_ISSUER");
string JWT_AUDIENCE = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
int JWT_EXPIRATION_MINUTES = 60;

builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = JWT_SECRET;
    options.Issuer = JWT_ISSUER;
    options.Audience = JWT_AUDIENCE;
    options.ExpirationMinutes = JWT_EXPIRATION_MINUTES;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET)),
        ValidateIssuer = true,
        ValidIssuer = JWT_ISSUER,
        ValidateAudience = true,
        ValidAudience = JWT_AUDIENCE,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migrações aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Erro ao aplicar migrações: {ex.Message}");
    }
}

app.Run();