using Newtonsoft.Json;
using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    /// <summary>
    /// API 요청 성공시 이곳에서 로그 관리 및 저장
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context); // 다음 미들웨어 실행

            // ResponseDTO 가져오기
            if (context.Items.TryGetValue("ResponseDTO", out var responseObj) &&
                responseObj is ResponseDTO<object> responseDto)
            {
                // MiddlewareHelper의 정적 메서드 사용하여 로그 저장
                await MiddlewareHelper.SaveLogToFileAsync(_logger, responseDto, responseDto.Success);
            }
            else
            {
                _logger.LogWarning("[LoggingMiddleware] ResponseDTO not found in HttpContext.Items.");
            }
        }
    }
}
