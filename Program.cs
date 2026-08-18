using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using ShopApi;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Добавление DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление Identity с использованием User и IdentityRole<Guid>
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()         
             .AllowCredentials();
    });
});

// Регистрация репозиториев
builder.Services.AddScoped<ICategory, RepositoryCategory>();
builder.Services.AddScoped<IProduct, RepositoryProduct>();
builder.Services.AddScoped<IOrder, RepositoryOreder>();
builder.Services.AddScoped<IOrderItem, RepositoryOrderItem>();
builder.Services.AddScoped<IEmailConfirmToken, EmailConfirmToken>();

// Регистрация сервисов
builder.Services.AddScoped<IAuthorization, AuthorizationServices>();
builder.Services.AddScoped<IServiceCategory, ServiceCategory>();
builder.Services.AddScoped<IServiceProduct, ServiceProduct>();
builder.Services.AddScoped<IServiceOrder, ServiceOrder>();
builder.Services.AddScoped<IServiceConfirmEmail, ServiceConfirmEmail>();

// Регистрация Email сервиса
builder.Services.AddScoped<IEmailService, EmailSerives>();

// Регистрация middleware для обработки исключений
builder.Services.AddScoped<GlobalExceptionHandler>();

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ShopApi_";
});

// Регистрация  IConnectionMultiplexer для доступа к командам к Redis-командам
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration);
});

// Регистрация сервиса кеширования
builder.Services.AddScoped<ICacheService, RedisCacheServices>();
var paymentBaseUrl = builder.Configuration["PaymentService:BaseUrl"];
if (string.IsNullOrEmpty(paymentBaseUrl))
{
    throw new InvalidOperationException("PaymentService:BaseUrl не задан");
}

builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
{
    client.BaseAddress = new Uri(paymentBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler((services, request) =>
{
    var logger = services.GetRequiredService<ILogger<PaymentClient>>();
    return HttpPolicyExtensions 
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                logger.LogWarning("Повторная попытка {RetryAttempt} через {Delay} мс",
                    retryAttempt, timespan.TotalMilliseconds);
            });
});

// Добавление контроллеров с настройкой JSON для обработки циклических ссылок
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnetcore/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Создание ролей и первого админа при старте приложения                                                                                                                                                                             
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var context = services.GetRequiredService<AppDbContext>();

    try
    {
        // Применяем миграции при старте приложения
        context.Database.Migrate();
        Console.WriteLine("Миграции применены успешно");

        // Создаем роли если не существуют (проверяем каждую отдельно)
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            Console.WriteLine("Роль Admin создана");
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            Console.WriteLine("Роль User создана");
        }

        if (!await roleManager.RoleExistsAsync("Manager"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Manager"));
            Console.WriteLine("Роль Manager создана");
        }

        // Создаем админа если не существует                                                                                                                                                                                         
        var existingAdmin = await userManager.FindByEmailAsync("denis.shabalin2000@gmail.com");
        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                UserName = "denis.shabalin2000@gmail.com",
                Email = "denis.shabalin2000@gmail.com",
                FirstName = "Super",
                EmailConfirmed = true,
                RegisterAt = DateTime.UtcNow,
                DeliveryAddress = "Admin Office"
            };

            var result = await userManager.CreateAsync(adminUser, "AdminPass123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin"); 
        }
        else
        {
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
            }
        }
    }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при создании ролей и админа: {ex.Message}");
    }
}
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Использование CORS
app.UseCors("AllowAll");

// Глобальная обработка исключений
app.UseMiddleware<GlobalExceptionHandler>();

// Использование Authentication и Authorization (важен порядок!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
