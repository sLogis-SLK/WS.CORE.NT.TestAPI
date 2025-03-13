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
        private MetaDTO? _meta;
        private Stopwatch? _stopwatch;
        private HttpContext _context;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _meta = null;
            _stopwatch = null;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context); // 다음 미들웨어 호출

            _meta = context.Items.ContainsKey("MetaDTO") ? context.Items["MetaDTO"] as MetaDTO : null;
            _stopwatch = context.Items.ContainsKey("Stopwatch") ? context.Items["Stopwatch"] as Stopwatch : null;
            _context = context;

            if (_meta != null)
            {
                string logData = JsonConvert.SerializeObject(new
                {
                    Timestamp = _meta.ServerTimeStamp,
                    JobUUID = _meta.JobUUID,
                    RequestURL = _meta.RequestURL,
                    RequestIP = _meta.RequestIP,
                    ExecutionTime = _meta.ExecutionTime,
                    StatusCode = _meta.StatusCode,
                    meta = _meta
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
                
                _meta.StatusCode = StatusCodes.Status500InternalServerError;

                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(_context, "Error saving log to file", false, _stopwatch, _meta), _context);
            }
        }
    }
}
