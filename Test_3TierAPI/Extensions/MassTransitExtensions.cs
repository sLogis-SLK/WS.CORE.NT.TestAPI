using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SLK.Orchestration.API.Extensions
{
    /// <summary>
    /// 간단한 MassTransit 자동화 확장
    /// 모든 솔루션에서 동일하게 사용 가능
    /// </summary>
    public static class MassTransitExtensions
    {
        /// <summary>
        /// 표준 MassTransit 구성을 추가합니다.
        /// </summary>
        /// <param name="services">서비스 컬렉션</param>
        /// <param name="configuration">구성 객체</param>
        /// <param name="consumerAssembly">Consumer가 있는 어셈블리 (기본값: 호출하는 어셈블리)</param>
        /// <returns>서비스 컬렉션</returns>
        public static IServiceCollection AddStandardMassTransit(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly? consumerAssembly = null)
        {
            // Consumer 어셈블리가 지정되지 않으면 호출하는 어셈블리 사용
            consumerAssembly ??= Assembly.GetCallingAssembly();

            services.AddMassTransit(x =>
            {
                // Consumer 자동 스캔 등록
                x.AddConsumers(consumerAssembly);

                // 표준 엔드포인트 설정 적용
                x.AddConfigureEndpointsCallback((name, cfg) =>
                {
                    if (cfg is IRabbitMqReceiveEndpointConfigurator rmq)
                    {
                        // WMS 특성에 맞는 동기 처리 설정
                        rmq.PrefetchCount = 1;
                        rmq.ConcurrentMessageLimit = 1;

                        // 우선순위 큐 설정
                        rmq.SetQueueArgument("x-max-priority", 100);

                        // 표준 재시도 정책 (DB Lock 등을 고려)
                        rmq.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    }
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    // appsettings.json에서 연결 정보 가져오기
                    var connectionString = configuration.GetConnectionString("RabbitMQ");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("RabbitMQ 연결 문자열이 설정되지 않았습니다.");
                    }

                    cfg.Host(connectionString);

                    // 자동 엔드포인트 구성
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        /// <summary>
        /// 커스텀 설정이 가능한 MassTransit 구성을 추가합니다.
        /// </summary>
        /// <param name="services">서비스 컬렉션</param>
        /// <param name="configuration">구성 객체</param>
        /// <param name="configureEndpoints">엔드포인트 설정 액션</param>
        /// <param name="consumerAssembly">Consumer가 있는 어셈블리</param>
        /// <returns>서비스 컬렉션</returns>
        public static IServiceCollection AddCustomMassTransit(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<string, IRabbitMqReceiveEndpointConfigurator>? configureEndpoints = null,
            Assembly? consumerAssembly = null)
        {
            consumerAssembly ??= Assembly.GetCallingAssembly();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(consumerAssembly);

                x.AddConfigureEndpointsCallback((name, cfg) =>
                {
                    if (cfg is IRabbitMqReceiveEndpointConfigurator rmq)
                    {
                        // 기본 설정
                        rmq.PrefetchCount = 1;
                        rmq.ConcurrentMessageLimit = 1;
                        rmq.SetQueueArgument("x-max-priority", 100);
                        rmq.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                        // 커스텀 설정 적용
                        configureEndpoints?.Invoke(name, rmq);
                    }
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString("RabbitMQ");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("RabbitMQ 연결 문자열이 설정되지 않았습니다.");
                    }

                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}

/*
=== 사용 방법 ===

1. appsettings.json에 추가:
{
  "ConnectionStrings": {
    "RabbitMQ": "amqp://masstransit_user:its0622@localhost:5672"
  }
}

2. Program.cs에서 사용:

// 기본 사용법
builder.Services.AddStandardMassTransit(builder.Configuration);

// 또는 특정 어셈블리 지정
builder.Services.AddStandardMassTransit(builder.Configuration, typeof(SomeConsumer).Assembly);

// 또는 커스텀 설정이 필요한 경우
builder.Services.AddCustomMassTransit(
    builder.Configuration,
    (name, cfg) =>
    {
        // 특정 Consumer만 다른 설정 적용
        if (name.Contains("priority"))
        {
            cfg.PrefetchCount = 10;
            cfg.ConcurrentMessageLimit = 5;
        }
    });
*/