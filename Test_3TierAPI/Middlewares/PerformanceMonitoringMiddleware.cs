using System.Diagnostics;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context); // 다음 미들웨어 호출

            // MetaDTO 및 Stopwatch 가져오기
            if (context.Items.TryGetValue("Stopwatch", out object? stopwatchObj) && stopwatchObj is Stopwatch stopwatch)
            {
                stopwatch.Stop();
                long executionTimeMs = stopwatch.ElapsedMilliseconds;   // 실행시간(ms)

                // metadto 가져와서 실행 시간 저장
                if(!context.Items.TryGetValue("MetaDTO", out object? metaObj) || metaObj is not MetaDTO meta)
                {
                    meta = new MetaDTO();
                    context.Items["MetaDTO"] = meta;
                }
                meta.ExecutionTime = $"{executionTimeMs}ms";    // 실행시간 기록

                // jobuuid 가져오기
                string jobUUID = context.Items.TryGetValue("JobUUID", out object? jobUUIDObj) && jobUUIDObj is string uuid
                    ? uuid
                    : "Unknown";    // job uuid가 없으면 Unknown으로 처리

                //// 응답 헤더에 실행 시간 추가
                //context.Response.Headers["X-Execution-Time-ms"] = executionTimeMs.ToString();

                // 실행 시간 로그 기록
                _logger.LogInformation("[Performance] JobUUID: {JobUUID}, {Method} {Path} executed in {ExecutionTime}ms",
                    jobUUID, context.Request.Method, context.Request.Path, executionTimeMs);

                //  5초(5000ms) 이상 걸린 요청은 Warning 로그로 기록하여 성능 모니터링
                if (executionTimeMs > 5000)
                {
                    _logger.LogWarning("[Performance Warning] JobUUID: {JobUUID}, Slow request detected: {Method} {Path} took {ExecutionTime}ms",
                        jobUUID, context.Request.Method, context.Request.Path, executionTimeMs);
                }
            }
        }
    }
}
