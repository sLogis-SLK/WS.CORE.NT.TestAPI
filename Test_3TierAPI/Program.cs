using Test_3TierAPI.Middlewares;
using Test_3TierAPI.Services.공통;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<FrmTRCOM00001Service>();

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

//// 5. 성능 모니터링
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
