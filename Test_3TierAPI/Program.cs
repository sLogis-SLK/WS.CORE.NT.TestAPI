using Newtonsoft.Json;
using Test_3TierAPI.ActionFilters;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.Middlewares;
using Test_3TierAPI.Repositories;
using Test_3TierAPI.Services.공통;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Action Filter 추가
    options.Filters.Add<ApiResponseFilter>();
});

// Newtonsoft.Json을 기본 직렬화 라이브러리로 지정
// DataTable 객체를 자동으로 직렬화 할 수 있도록 하기 위함
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // DataTable 직렬화 가능하도록 설정
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

/// DI 등록
// services
builder.Services.AddScoped<FrmTRCOM00001Service>();

// infra
builder.Services.AddSingleton<DBConnectionFactory>();
builder.Services.AddScoped<DatabaseTransactionManager>();

// Repository
builder.Services.AddScoped<TestRepository>();
/// DI등록 끝

var app = builder.Build();

// 미들웨어 모음
// 0. 추후, 인증 및 권한 부여 미들웨어 추가 예정

// 1. 예외 처리 및 초기 Items 생성 미들웨어
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. API 로그 기록
app.UseMiddleware<LoggingMiddleware>();

// 3. Rate Limit 검사 (요청 초과 시 차단)
app.UseMiddleware<RateLimitMiddleware>();

// 4. 요청 데이터 유효성 검사
app.UseMiddleware<RequestValidationMiddleware>();

//// 5. 성능 모니터링 : 사용 안함. action filter로 대체
//app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

