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

            // ✅ ResponseDTO 가져오기
            context.Items.TryGetValue("ResponseDTO", out var responseObj);
            if (responseObj is ResponseDTO<object> responseDto)
            {
                // ✅ 로그 데이터 생성
                string logData = JsonConvert.SerializeObject(new
                {
                    Timestamp = responseDto.Meta?.ServerTimeStamp,
                    JobUUID = responseDto.JobUUID,
                    RequestURL = responseDto.Meta?.RequestURL,
                    RequestIP = responseDto.Meta?.RequestIP,
                    ExecutionTime = responseDto.Meta?.ExecutionTime,
                    StatusCode = responseDto.Meta?.StatusCode,
                    Response = responseDto
                });

                _logger.LogInformation($"[API Log] : {logData}");

                // ✅ 로그를 파일에 저장
                await SaveLogToFile(logData);
            }
        }

        /// <summary>
        /// 로그 데이터를 파일에 저장하는 함수 (BufferedStream을 활용하여 성능 최적화)
        /// </summary>
        private async Task SaveLogToFile(string logData)
        {
            try
            {
                string logDirectory = @"C:\APILogs";
                string logFileName = $"{DateTime.UtcNow:yyyyMMdd}.log";
                string logFilePath = Path.Combine(logDirectory, logFileName);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // ✅ BufferedStream을 활용하여 파일 저장 성능 향상
                await using var fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
                await using var writer = new StreamWriter(new BufferedStream(fileStream));
                await writer.WriteLineAsync(logData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving log to file: {Message}", ex.Message);
            }
        }
    }
}
