using Newtonsoft.Json;
using System.Diagnostics;
using Test_3TierAPI.Exceptions;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Helpers
{
    /// <summary>
    /// Middleware에서 사용하는 Helper 클래스
    /// </summary>
    public static class MiddlewareHelper
    {
        /// <summary>
        /// 로그를 파일로 저장
        /// 로그 저장시 데이터는 null로 저장
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="responseDTO"></param>
        /// <param name="bIsSuccess"></param>
        public static async Task SaveLogToFileAsync(ILogger logger, ResponseDTO<object> responseDTO, bool bIsSuccess)
        {
            try
            {
                string logDirectory = @"C:\APILogs";
                string logFileName = $"{DateTime.UtcNow:yyyyMMdd}.log"; // 로그 파일명은 날짜별로 생성
                string logFilePath = Path.Combine(logDirectory, logFileName);

                // 없으면 디렉토리 생성 (비동기 지원 안 함)
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                responseDTO.Data = null; // 데이터는 로그에 저장하지 않음

                // JSON 변환 (동기)
                string jsonLog = JsonConvert.SerializeObject(new
                {
                    LogType = bIsSuccess ? "Success Info" : "ErrorLog",
                    TimeStamp = responseDTO.Meta.ServerTimeStamp,
                    Response = responseDTO
                });

                // ILogger를 활용하여 로그 출력 (비동기 필요 없음)
                if (!bIsSuccess)
                    logger.LogError($"[API Exception] {jsonLog}");
                else
                    logger.LogInformation($"[API Log] {jsonLog}");

                // 비동기 파일 쓰기
                await File.AppendAllTextAsync(logFilePath, jsonLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error saving log to file : {ex.Message}");
            }
        }


        /// <summary>
        /// Exception에 따른 HTTP 상태 코드 반환
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static int GetStatusCode(Exception ex)
        {
            if (ex is TooManyRequestsException) return StatusCodes.Status429TooManyRequests;
            if (ex is ArgumentException) return StatusCodes.Status400BadRequest;
            if (ex is UnauthorizedAccessException || ex is KeyNotFoundException) return StatusCodes.Status401Unauthorized;

            return StatusCodes.Status500InternalServerError; // 기본값
        }
    }
}
