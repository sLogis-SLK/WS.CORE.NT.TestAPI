using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            // JobUUID 및 MetaDTO 초기화 (항상 새로 생성)
            var jobUUID = Guid.NewGuid().ToString();
            var meta = new MetaDTO
            {
                JobUUID = jobUUID,
                ServerTimeStamp = DateTime.UtcNow.ToString("o"),
                RequestIP = context.Connection.RemoteIpAddress?.ToString(),
                RequestURL = context.Request.Path
            };

            // Context에 저장
            context.Items["MetaDTO"] = meta;
            context.Items["JobUUID"] = jobUUID;
            context.Items["bIsDev"] = _env.IsDevelopment();

            // 요청 시간 측정을 위한 Stopwatch 시작
            var stopwatch = Stopwatch.StartNew();
            context.Items["Stopwatch"] = stopwatch;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Stopwatch 정지
                stopwatch.Stop();

                // 예외 로그 기록
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

                // ErrorResponseDTO가 있으면 즉시 반환
                if (context.Items.TryGetValue("ErrorResponseDTO", out var errorResponse))
                {
                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }

                // catch 블록에서만 MetaDTO를 context.Items에서 가져옴
                if (context.Items.TryGetValue("MetaDTO", out var existingMeta) && existingMeta is MetaDTO errorMeta)
                {
                    errorMeta.StatusCode = MiddlewareHelper.GetStatusCode(ex);
                    meta = errorMeta;
                }
                else
                {
                    meta = new MetaDTO
                    {
                        JobUUID = jobUUID,
                        ServerTimeStamp = DateTime.UtcNow.ToString("o"),
                        RequestIP = context.Connection.RemoteIpAddress?.ToString(),
                        RequestURL = context.Request.Path,
                        StatusCode = MiddlewareHelper.GetStatusCode(ex)
                    };
                }

                // 예외 처리 응답 생성
                var errorMessage = $"Unhandled Error occurred: {ex.Message}";
                var errorResponseDTO = MiddlewareHelper.GetErrorResponse<object>(context, errorMessage, _env.IsDevelopment(), stopwatch, meta);

                context.Response.StatusCode = meta.StatusCode;
                await context.Response.WriteAsJsonAsync(errorResponseDTO);
            }
        }
    }
}
