using Newtonsoft.Json;
using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
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
            context.Items.TryGetValue("ResponseDTO", out var responseObj);
            if (responseObj is ResponseDTO<object> responseDto)
            {
                MiddlewareHelper.SaveLogToFile(_logger, responseDto, responseDto.Success);
            }
        }
    }
}
