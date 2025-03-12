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
            // MetaDTO 초기화 및 Job UUID 생성
            string JobUUID = Guid.NewGuid().ToString();

            MetaDTO meta = new MetaDTO
            {
                JobUUID = JobUUID,
                ServerTimeStamp = DateTime.UtcNow.ToString("o"),
                RequestIP = context.Connection.RemoteIpAddress?.ToString(),
                RequestURL = context.Request.Path
            };

            context.Items["MetaDTO"] = meta;            // metaDTO context에 저장
            context.Items["JobUUID"] = meta.JobUUID;    // JobUUID context에 저장

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            context.Items["Stopwatch"] = stopwatch;     // stopwatch context에 저장

            bool bIsDev = _env.IsDevelopment();
            context.Items["bIsDev"] = bIsDev;           // 개발 환경 여부 context에 저장

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"Unhandled exception : {ex.Message}", ex);

                if (context.Items.ContainsKey("ErrorResponseDTO"))          // errorResponse가 처리된 애들의 경우
                {
                    await context.Response.WriteAsJsonAsync(context.Items["ErrorResponseDTO"]);
                }
                else                                                        // errorResponse가 처리된 안된 애들
                {
                    string message = $"Unhandled Error occurred : {ex.Message}";
                    int statusCode = MiddlewareHelper.GetStatusCode(ex);

                    // MetaDTO 정리
                    MetaDTO? errorMetaDTO = null;
                    if (context.Items.ContainsKey("MetaDTO"))
                    {
                        errorMetaDTO = (MetaDTO?)context.Items["MetaDTO"];
                        errorMetaDTO.StatusCode = statusCode;

                        await context.Response.WriteAsJsonAsync(MiddlewareHelper.GetErrorResponse<object>(context, message, bIsDev, stopwatch, errorMetaDTO));
                    }
                    else
                    {
                        errorMetaDTO = new MetaDTO
                        {
                            JobUUID = JobUUID,
                            ServerTimeStamp = DateTime.UtcNow.ToString("o"),
                            RequestIP = context.Connection.RemoteIpAddress?.ToString(),
                            RequestURL = context.Request.Path,
                            StatusCode = statusCode
                        };

                        await context.Response.WriteAsJsonAsync(MiddlewareHelper.GetErrorResponse<object>(context, message, bIsDev, stopwatch, errorMetaDTO));  
                    }
                }
            }               
        }
    }
}
