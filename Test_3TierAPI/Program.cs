using Newtonsoft.Json;
using System.Net;
using Test_3TierAPI.ActionFilters;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.Middlewares;
using Test_3TierAPI.Repositories;
using Test_3TierAPI.Services;
using Test_3TierAPI.Services.공통;
using MassTransit;

using Test_3TierAPI.MassTransit.MQMessage;
using MassTransit.Transports.Fabric;
using MQ_Message;
using SLK.Orchestration.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 다른 ip에서 접근 허용
builder.WebHost.UseUrls("http://0.0.0.0:7080");

//// Add services to the container.
//builder.Services.AddControllers(options =>
//{
//    // 필터 순서는 OnActionExecuted 메서드의 경우 등록의 역순으로 실행됨 - 컨트롤러 실행 이후에 작동하는 함수
//    // 아래 순서로 등록하면:
//    // - OnActionExecuting: ApiResponseFilter → ResponseCompressionFilter
//    // - OnActionExecuted: ResponseCompressionFilter → ApiResponseFilter
//    options.Filters.Add<ResponseCompressionFilter>();   // 두 번째 실행
//    options.Filters.Add<ApiResponseFilter>();           // 첫 번째 실행
//});

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
builder.Services.AddScoped<DataService>();

// infra
builder.Services.AddSingleton<DBConnectionFactory>();
builder.Services.AddScoped<DatabaseTransactionManager>();

// Repository
builder.Services.AddScoped<TestRepository>();
builder.Services.AddScoped<DataRepository>();

// kestrel에 포트 바인딩 명시
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 7080); // HTTP 바인딩
});
/// DI등록 끝

//builder.Services.AddMassTransit(x =>
//{
//    // Consumer 등록 + Retry 정책 설정
//    x.AddConsumer<MQTestConsumer>(cfg =>
//    {
//        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // 3회 재시도, 5초 간격
//    });

//    x.AddConsumer<Message_MQ_ContainerConsumer>(cfg =>
//    {
//        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
//    });

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host("rabbitmq://localhost", h =>
//        {
//            h.Username("masstransit_user");
//            h.Password("its0622");
//        });

//        // 메시지 타입에 대한 Publish용 Exchange 이름 지정
//        cfg.Message<MQTest>(m =>
//        {
//            m.SetEntityName("slk.exchange.mqtest");
//        });

//        cfg.Message<Message_MQ_Container>(m =>
//        {
//            m.SetEntityName("slk.exchange.onlineordertest");
//        });

//        // MQTestConsumer용 Queue 설정
//        cfg.ReceiveEndpoint("slk.queue.mqtest", e =>
//        {
//            e.ConfigureConsumeTopology = false;

//            // 우선순위 큐 설정
//            e.SetQueueArgument("x-max-priority", 100);

//            // 동기 처리 설정 (우선순위 정확히 보장)
//            e.PrefetchCount = 1;
//            e.ConcurrentMessageLimit = 1;

//            // fanout Exchange 바인딩
//            e.Bind("slk.exchange.mqtest", x =>
//            {
//                x.ExchangeType = "fanout";
//            });

//            e.ConfigureConsumer<MQTestConsumer>(context);
//        });

//        // OnlineOrderTestConsumer용 Queue 설정
//        cfg.ReceiveEndpoint("slk.queue.onlineordertest", e =>
//        {
//            e.ConfigureConsumeTopology = false;

//            e.SetQueueArgument("x-max-priority", 100);

//            e.PrefetchCount = 1;
//            e.ConcurrentMessageLimit = 1;

//            e.Bind("slk.exchange.onlineordertest", x =>
//            {
//                x.ExchangeType = "fanout";
//            });

//            e.ConfigureConsumer<Message_MQ_ContainerConsumer>(context);
//        });
//    });
//});

// MassTransit 자동화 사용
builder.Services.AddStandardMassTransit(builder.Configuration);

// HttpClientFactory 설정
builder.Services.AddHttpClient("TestAPIGateway", client =>
{
    //client.BaseAddress = new Uri("http://172.16.32.83:6999"); // 실제 API 주소로 변경
    client.BaseAddress = new Uri("http://172.16.32.50:6999");      // 본인 ip로 변경
    client.Timeout = TimeSpan.FromSeconds(120);
});


var app = builder.Build();

app.UseStaticFiles();

// 미들웨어 순서
// 0. 추후, 인증 및 권한 부여 미들웨어 추가 예정

//// 1. 예외 처리 및 초기 Items 생성 미들웨어
//app.UseMiddleware<ExceptionHandlingMiddleware>();

//// 2. 요청 제한 검사 (요청 초과 시 차단)
//app.UseMiddleware<RateLimitMiddleware>();

//// 3. 요청 데이터 유효성 검사
//app.UseMiddleware<RequestValidationMiddleware>();

//// 4. RequestDTO의 Data의 디테일한 필드값에 대한 Valid 체크 하는 미들웨어
//app.UseMiddleware<FieldValidationMiddleware>();

//// 5. API 로그 기록 (로그 관리 개선된 미들웨어)
//app.UseMiddleware<LoggingMiddleware>();

//// 6. 성능 모니터링 : 사용 안함. action filter로 대체
//app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SLK NT Orchestration API");

        c.InjectJavascript("https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");

        // ↓↓↓ 여기가 핵심!!
        c.InjectJavascript("/swagger/custom-signalr.js");
    });
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();