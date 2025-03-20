using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Test_3TierAPI.Exceptions;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    /// <summary>
    /// RateLimitMiddleware
    /// 같은 ip에 대해서 요청 제한을 두는 미들웨어
    /// 추후 mac address 등으로 변경 가능 및 추가 관리 필요
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly IMemoryCache _cache;

        private const int RateLimitMax = 50;
        private const int RateLimitWindowSeconds = 60;

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger, IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            MetaDTO? metaDTO = context.Items.ContainsKey("MetaDTO") ? context.Items["MetaDTO"] as MetaDTO : new MetaDTO();

            try
            {
                // 클라이언트 식별자 가져오기
                string? clientId = GetClientId(context);

                if (clientId == null)
                    throw new InvalidOperationException("Clinet IP addresss not found");

                string cacheKey = $"RateLimit:{clientId}";

                // 캐시에서 RateLimitInfo 가져오기 (예외 발생 가능성 확인)
                RateLimitInfo? rateLimitInfo = null;
                try
                {
                    rateLimitInfo = _cache.GetOrCreate(cacheKey, entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RateLimitWindowSeconds);
                        return new RateLimitInfo { Remaining = RateLimitMax, Expiration = DateTime.UtcNow.AddSeconds(RateLimitWindowSeconds) };
                    });
                }
                catch (Exception cacheEx)
                {
                    _logger.LogError($"Cache access error: {cacheEx.Message}", cacheEx);
                }

                if (rateLimitInfo == null)
                {
                    _logger.LogError("RateLimitInfo is null, skipping rate limiting.");
                    await _next(context);
                    return;
                }

                if (rateLimitInfo.Remaining <= 0) throw new TooManyRequestsException("Rate limit exceeded");

                rateLimitInfo.Remaining--;
                _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromSeconds(RateLimitWindowSeconds));

                // MetaDTO 업데이트
                metaDTO.RateLimitRemaining = rateLimitInfo.Remaining;
                metaDTO.RateLimitMax = RateLimitMax;
                context.Items["MetaDTO"] = metaDTO;

                await _next(context);
            }
            catch (Exception ex)
            {
                throw new Exception("RateLimitMiddleware Error", ex); // InnerException 유지
            }
        }

        private string? GetClientId(HttpContext context) => context.Connection.RemoteIpAddress?.ToString();

        private class RateLimitInfo
        {
            public int Remaining { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
