using Newtonsoft.Json;
using System.Net;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.Repositories;
using Test_3TierAPI.Services;
using Test_3TierAPI.Services.공통;
using MassTransit;
using SLK.Orchestration.API.Extensions;
using Test_3TierAPI.Helpers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =======================================================================
// Kestrel / Host
// =======================================================================
builder.WebHost.UseUrls("http://0.0.0.0:7080");
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 7080);
});

// =======================================================================
// Controllers + Newtonsoft.Json
// =======================================================================
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

// =======================================================================
// Swagger (Bearer 입력 + Custom JS)
// =======================================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Test 3-Tier API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =======================================================================
// DI
// =======================================================================
builder.Services.AddScoped<FrmTRCOM00001Service>();
builder.Services.AddScoped<DataService>();

builder.Services.AddSingleton<DBConnectionFactory>();
builder.Services.AddScoped<DatabaseTransactionManager>();

builder.Services.AddScoped<TestRepository>();
builder.Services.AddScoped<DataRepository>();

// =======================================================================
// MassTransit
// =======================================================================
builder.Services.AddStandardMassTransit(builder.Configuration);

// =======================================================================
// HttpClient + Authorization Forwarding (핵심)
// =======================================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthorizationHandler>();

builder.Services.AddHttpClient("TestAPIGateway", client =>
{
    client.BaseAddress = new Uri("http://172.16.32.50:6999");
    client.Timeout = TimeSpan.FromSeconds(120);
})
.AddHttpMessageHandler<ForwardAuthorizationHandler>();

// =======================================================================
// App
// =======================================================================
var app = builder.Build();

app.UseStaticFiles();

// =======================================================================
// Swagger UI + JS Inject
// =======================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test 3-Tier API");

        // ✅ 외부 JS
        c.InjectJavascript(
            "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");

        // ✅ 커스텀 JS (wwwroot/swagger/custom-signalr.js)
        c.InjectJavascript("/swagger/custom-signalr.js");
    });
}

// =======================================================================
// Endpoints
// =======================================================================
app.MapControllers();
app.Run();
