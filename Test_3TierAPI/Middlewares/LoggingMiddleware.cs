using Newtonsoft.Json;
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
            await _next(context); // 다음 미들웨어 호출

            if(context.Items.ContainsKey("MetaDTO") && context.Items["MetaDTO"] is MetaDTO meta)
            {
                string logData = JsonConvert.SerializeObject(new
                {
                    Timestamp = meta.ServerTimeStamp,
                    JobUUID = meta.JobUUID,
                    RequestURL = meta.RequestURL,
                    RequestIP = meta.RequestIP,
                    ExecutionTime = meta.ExecutionTime,
                    StatusCode = meta.StatusCode,
                    meta = meta
                });

                _logger.LogInformation($"[API Log] : {logData}");   

                await SaveLogToFile(logData);   // 로그 파일 텍스트 파일에 저장
            }
        }

        public async Task SaveLogToFile(string logData)
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
                
                await File.AppendAllTextAsync(logFilePath, logData + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving log to file: {ex.Message}", ex);
            }
        }
    }
}
