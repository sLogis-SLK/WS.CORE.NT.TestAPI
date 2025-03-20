using Newtonsoft.Json;
using System.Text;
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
        /// 로그를 날짜별 폴더 및 엔드포인트별 파일로 저장
        /// </summary>
        /// <param name="logger">로거</param>
        /// <param name="responseDTO">응답 DTO</param>
        /// <param name="bIsSuccess">성공 여부</param>
        /// <param name="exception">예외 객체(선택적)</param>
        public static async Task SaveLogToFileAsync(
            ILogger logger, 
            ResponseDTO<object> responseDTO, 
            bool bIsSuccess, 
            Exception? exception = null)
        {
            try
            {
                // 1. 로그 디렉토리 구조 생성
                string baseLogDirectory = @"C:\APILogs";
                string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string logDirectory = Path.Combine(baseLogDirectory, dateFolder);
                
                // 2. 엔드포인트 추출
                string endpoint = ExtractEndpoint(responseDTO.Meta?.RequestURL);
                string logFileName = $"{endpoint}.log";
                string logFilePath = Path.Combine(logDirectory, logFileName);
                
                // 3. 로그 폴더가 없으면 생성
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // 4. 로그 내용 생성
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{(bIsSuccess ? "INFO" : "ERROR")}] [UUID:{responseDTO.JobUUID}]");
                
                // 메타 정보 추가
                if (responseDTO.Meta != null)
                {
                    logBuilder.AppendLine($"Requester: {responseDTO.Meta.Requester ?? "N/A"}");
                    logBuilder.AppendLine($"Request URL: {responseDTO.Meta.RequestURL ?? "N/A"}");
                    logBuilder.AppendLine($"Request IP: {responseDTO.Meta.RequestIP ?? "N/A"}");
                    logBuilder.AppendLine($"Execution Time: {responseDTO.Meta.ExecutionTime ?? "N/A"}");
                    
                    if (!string.IsNullOrEmpty(responseDTO.Meta.ErrorDetail))
                    {
                        logBuilder.AppendLine($"Error Detail: {responseDTO.Meta.ErrorDetail}");
                    }
                }
                
                // 예외 정보 추가
                if (exception != null)
                {
                    logBuilder.AppendLine($"Exception Type: {exception.GetType().Name}");
                    logBuilder.AppendLine($"Exception Message: {exception.Message}");
                    logBuilder.AppendLine($"Stack Trace: {exception.StackTrace}");
                }
                
                // 응답 정보 추가 (Data 부분은 제외)
                var logResponseDto = new ResponseDTO<object>
                {
                    JobUUID = responseDTO.JobUUID,
                    Success = responseDTO.Success,
                    StatusCode = responseDTO.StatusCode,
                    Message = responseDTO.Message,
                    TableCount = responseDTO.TableCount,
                    Meta = null // 이미 위에서 중요 메타 정보는 추출했으므로 제외
                };
                
                logBuilder.AppendLine($"Response: {JsonConvert.SerializeObject(logResponseDto)}");
                logBuilder.AppendLine(new string('-', 80)); // 구분선 추가
                
                // 5. 파일에 로그 저장 (파일 쓰기 동시성 제어는 운영체제에 맡김)
                await File.AppendAllTextAsync(logFilePath, logBuilder.ToString());
                
                // 6. 로거에도 기록
                if (bIsSuccess)
                {
                    logger.LogInformation($"[API:{endpoint}] Job:{responseDTO.JobUUID} completed successfully");
                }
                else
                {
                    logger.LogError($"[API:{endpoint}] Job:{responseDTO.JobUUID} failed: {responseDTO.Message}");
                }
            }
            catch (Exception ex)
            {
                // 로그 저장 실패 시 로거에만 기록 (에러 전파 방지)
                logger.LogError(ex, "Failed to save log file");
            }
        }

        /// <summary>
        /// URL에서 엔드포인트 이름을 추출
        /// </summary>
        private static string ExtractEndpoint(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "unknown-endpoint";
            }

            // URL에서 마지막 경로 부분을 추출 (/api/controller/action -> action)
            string[] parts = url.TrimEnd('/').Split('/');
            string endpoint = parts.Length > 0 ? parts[^1] : "unknown-endpoint";

            // 쿼리 문자열 제거
            int queryIndex = endpoint.IndexOf('?');
            if (queryIndex > 0)
            {
                endpoint = endpoint.Substring(0, queryIndex);
            }

            // 컨트롤러 이름을 접두어로 추가
            if (parts.Length > 1)
            {
                string controller = parts[^2];
                return $"{controller}_{endpoint}";
            }

            return endpoint;
        }

        // MiddlewareHelper.cs 파일에 추가
        public static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                TooManyRequestsException => StatusCodes.Status429TooManyRequests,
                FieldValidationException => StatusCodes.Status400BadRequest,
                ArgumentException or InvalidOperationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                JsonException => StatusCodes.Status400BadRequest,
                TimeoutException => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status500InternalServerError // 기본값
            };
        }
    }
}