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
            Stopwatch? stopwatch = context.Items.ContainsKey("Stopwatch") ? context.Items["Stopwatch"] as Stopwatch : new Stopwatch();
            bool bIsDev = context.Items.ContainsKey("bIsDev") ? (bool)context.Items["bIsDev"] : false;
            MetaDTO? metaDTO = context.Items.ContainsKey("MetaDTO") ? context.Items["MetaDTO"] as MetaDTO : new MetaDTO();

            try
            {
                // 클라이언트 식별자(IP 주소)를 가져옴
                string? clientId = GetClientId(context);

                if (clientId == null)
                {
                    _logger.LogWarning("Client IP address not found");
                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Client IP address not found", bIsDev, stopwatch, metaDTO), context);
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
                    
                    metaDTO.StatusCode = StatusCodes.Status429TooManyRequests;

                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Too Many Requests. Please try again later.", bIsDev, stopwatch, metaDTO), context);
                    return;
                }

                // 요청 허용 -> 남은 횟수 감소
                rateLimitInfo.Remaining--; // 요청 횟수 감소
                _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromSeconds(RateLimitWindowSeconds)); // 캐시 업데이트

                // MetaDTO에 RateLimit 정보 저장
                metaDTO.RateLimitRemaining = rateLimitInfo.Remaining;
                metaDTO.RateLimitMax = RateLimitMax;

                await _next(context); // 다음 미들웨어로 요청 전달
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in RateLimitMiddleware: {ex.Message}", ex);

                // 500 Internal Server Error 응답 반환
                metaDTO.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "An unexpected error occurred in rate limiting.", bIsDev, stopwatch, metaDTO), context);
                return;
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
