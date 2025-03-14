using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly IMemoryCache _cache;

        private const int RateLimitMax = 10;
        private const int RateLimitWindowSeconds = 60;

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
                // 클라이언트 식별자 가져오기
                string? clientId = GetClientId(context);

                if (clientId == null)
                {
                    _logger.LogWarning("Client IP address not found");
                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Client IP address not found", bIsDev, stopwatch, metaDTO), context);
                    return;
                }

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

                if (rateLimitInfo.Remaining <= 0)
                {
                    _logger.LogWarning($"Rate limit exceeded for client {clientId}");

                    metaDTO.StatusCode = StatusCodes.Status429TooManyRequests;
                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Too Many Requests. Please try again later.", bIsDev, stopwatch, metaDTO), context);
                    return;
                }

                rateLimitInfo.Remaining--;
                _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromSeconds(RateLimitWindowSeconds));

                metaDTO.RateLimitRemaining = rateLimitInfo.Remaining;
                metaDTO.RateLimitMax = RateLimitMax;
                context.Items["MetaDTO"] = metaDTO;

                await _next(context);
            }
            catch (ObjectDisposedException disposedEx)
            {
                _logger.LogError($"[ObjectDisposedException] Disposed object: {disposedEx.ObjectName} - {disposedEx.Message}\nStackTrace: {disposedEx.StackTrace}", disposedEx);
                metaDTO.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "A disposed object was accessed in rate limiting.", bIsDev, stopwatch, metaDTO), context);
            }
            catch (NullReferenceException nullEx)
            {
                _logger.LogError($"[NullReferenceException] Possible null object access: {nullEx.Message}", nullEx);
                metaDTO.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "A null reference error occurred in rate limiting.", bIsDev, stopwatch, metaDTO), context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in RateLimitMiddleware: {ex.Message}", ex);
                metaDTO.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "An unexpected error occurred in rate limiting.", bIsDev, stopwatch, metaDTO), context);
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
