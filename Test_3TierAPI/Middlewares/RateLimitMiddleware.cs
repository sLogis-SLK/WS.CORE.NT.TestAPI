using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next; // 다음 미들웨어로 요청을 전달하는 Delegate
        private readonly ILogger<RateLimitMiddleware> _logger; // 로깅을 위한 ILogger
        private readonly IMemoryCache _cache; // 클라이언트 요청 횟수를 저장할 메모리 캐시

        private const int RateLimitMax = 10; // 최대 요청 횟수 (예 : 10회 / 분)
        private const int RateLimitWindowSeconds = 60; // 요청 제한 시간 (60초)

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger, IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew(); // 요청 실행 시간 측정 시작

            try
            {
                // 클라이언트 식별자(IP 주소)를 가져옴
                string? clientId = GetClientId(context);

                if (clientId == null)
                {
                    _logger.LogWarning("Client IP address not found");
                    await MiddlewareHelper.WriteErrorResponse(context, "Client IP address not found", stopwatch);
                    return;
                }

                // 캐시 키 : 클라이언트 요청 제한을 관리하기 위해 사용 (예 : "RateLimit:192.168.0.1")
                string cacheKey = $"RateLimit:{clientId}";

                // 현재 클라이언트의 남은 요청 횟수를 조회 (캐시에 없으면 새로운 RateLimit 정보 생성)
                RateLimitInfo? rateLimitInfo = _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RateLimitWindowSeconds); // 캐시 만료 시간 설정 : 60초 후 만료
                    return new RateLimitInfo { Remaining = RateLimitMax, Expiration = DateTime.UtcNow.AddSeconds(RateLimitWindowSeconds) };
                });

                // 요청 횟수 초과 여부 확인
                if (rateLimitInfo.Remaining <= 0)
                {
                    _logger.LogWarning($"Rate limit exceeded for client {clientId}");

                    await MiddlewareHelper.WriteErrorResponse(context, "Too Many Requests. Please try again later.", stopwatch);
                    return;
                }

                // 요청 허용 -> 남은 횟수 감소
                rateLimitInfo.Remaining--; // 요청 횟수 감소
                _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromSeconds(RateLimitWindowSeconds)); // 캐시 업데이트

                // MetaDTO를 context.Items에 저장
                if (!context.Items.ContainsKey("MetaDTO"))
                {
                    context.Items["MetaDTO"] = new MetaDTO();
                }

                // MetaDTO에 RateLimit 정보 저장
                MetaDTO? metaDto = context.Items["MetaDTO"] as MetaDTO;
                metaDto.RateLimitRemaining = rateLimitInfo.Remaining;
                metaDto.RateLimitMax = RateLimitMax;
                metaDto.ExecutionTime = $"{stopwatch.ElapsedMilliseconds}ms";
                metaDto.ServerTimeStamp = DateTime.UtcNow.ToString("o");

                await _next(context); // 다음 미들웨어로 요청 전달
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in RateLimitMiddleware: {ex.Message}", ex);

                // 500 Internal Server Error 응답 반환
                await MiddlewareHelper.WriteErrorResponse(context, "An unexpected error occurred in rate limiting.", stopwatch);
            }
        }

        private string? GetClientId(HttpContext context) => context.Connection.RemoteIpAddress?.ToString();

        private class RateLimitInfo
        {
            public int Remaining { get; set; } // 남은 요청 횟수
            public DateTime Expiration { get; set; } // 만료 시간 (초기화 시점)
        }
    }
}
