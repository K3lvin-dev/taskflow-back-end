using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskFlowAPI.src.entity;
using TaskFlowAPI.src.entity.board.services;
using TaskFlowAPI.src.entity.task.services;
using TaskFlowAPI.src.entity.user.services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName.ToLower();
Console.WriteLine($"🌍 Ambiente: {environment}");

// Melhor detecção de container - verifica se ASPNETCORE_URLS contém "+:80" (padrão Docker)
var aspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "";
var isContainer = aspNetUrls.Contains("+:80") || aspNetUrls.Contains("0.0.0.0:80");

Console.WriteLine($"🐳 Container: {(isContainer ? "Sim" : "Não")}");

if (!isContainer)
{
    // Executando localmente - carregar .env
    var envFile = environment == "production" ? ".env.prd" : ".env.dev";
    var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
    var envPath = Path.Combine(projectRoot, envFile);

    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        Console.WriteLine($"✅ {envFile} carregado");
    }
    else
    {
        Console.WriteLine($"⚠️ {envFile} não encontrado");
    }
}
else
{
    Console.WriteLine($"📦 Usando variáveis do container");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();


// Configuração do banco
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    // Construir connection string a partir de variáveis individuais
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "taskflow_dev";
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "123456";
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};";
}

Console.WriteLine($"🗄️ Database: {connectionString.Split(';').FirstOrDefault(x => x.StartsWith("Database="))}");
Console.WriteLine($"🏠 Host: {connectionString.Split(';').FirstOrDefault(x => x.StartsWith("Host="))}");
Console.WriteLine(connectionString);

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Configuração JWT
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "sua-chave-super-secreta-que-deve-ter-pelo-menos-32-caracteres";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "TaskFlowAPI";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "TaskFlowUsers";

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
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

// Aplicar migrações automaticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("✅ Migrações aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao aplicar migrações: {ex.Message}");
        // Fallback para criação simples se não houver migrações
        try
        {
            context.Database.EnsureCreated();
            Console.WriteLine("✅ Banco criado com EnsureCreated como fallback");
        }
        catch (Exception ex2)
        {
            Console.WriteLine($"❌ Erro ao criar banco: {ex2.Message}");
        }
    }
}

app.Run();