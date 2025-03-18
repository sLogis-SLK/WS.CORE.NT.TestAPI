using Newtonsoft.Json;
using System.Diagnostics;
using Test_3TierAPI.Exceptions;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Helpers
{
    public static class MiddlewareHelper
    {
        public static void SaveLogToFile(ILogger logger, ResponseDTO<object> responseDTO, bool bIsSuccess)
        {
            try
            {
                string logDirectory = @"C:\APILogs";
                string logFileName = $"{DateTime.UtcNow:yyyyMMdd}.log"; // 로그 파일명은 날짜별로 생성

                string logFilePath = Path.Combine(logDirectory, logFileName);

                // 없으면 파일 생성 - 아마도 매일 날짜가 바뀌면 새로 생성되는 기능도 같이 수행
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                responseDTO.Data = null; // 데이터는 로그에 저장하지 않음

                string jsonLog = JsonConvert.SerializeObject(new
                {
                    LogType = bIsSuccess ? "Success Info" : "ErrorLog",
                    TimeStamp = responseDTO.Meta.ServerTimeStamp,
                    Response = responseDTO
                });

                // ILogger를 활용하여 로그 출력
                if (bIsSuccess == false)
                    logger.LogError($"[API Exception] {jsonLog}");
                else
                    logger.LogInformation($"[API Log] {jsonLog}");

                // 파일에 로그 저장
                File.AppendAllText(logFilePath, jsonLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error saving log to file : {ex.Message}");
            }
        }

        public static int GetStatusCode(Exception ex)
        {
            if (ex is TooManyRequestsException) return StatusCodes.Status429TooManyRequests;
            if (ex is ArgumentException) return StatusCodes.Status400BadRequest;
            if (ex is UnauthorizedAccessException || ex is KeyNotFoundException) return StatusCodes.Status401Unauthorized;

            return StatusCodes.Status500InternalServerError; // 기본값
        }
    }
}
