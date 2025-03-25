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
        /// 로그를 엔드포인트별 폴더 및 날짜별 파일로 저장
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
                // 1. 엔드포인트 추출
                string endpoint = ExtractEndpoint(responseDTO.Meta?.RequestURL);

                // 2. 로그 디렉토리 구조 생성 (엔드포인트별 폴더)
                string baseLogDirectory = @"C:\APILogs";
                string endpointFolder = endpoint;
                string endpointDirectory = Path.Combine(baseLogDirectory, endpointFolder);

                // 3. 날짜 기반 파일명 생성
                string dateString = DateTime.Now.ToString("yyyy-MM-dd");
                string logFileName = $"{dateString}.log";
                string logFilePath = Path.Combine(endpointDirectory, logFileName);

                

                // 4. 로그 폴더가 없으면 생성
                if (!Directory.Exists(endpointDirectory))
                {
                    Directory.CreateDirectory(endpointDirectory);
                }

                // 5. 로그 내용 생성
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{(bIsSuccess ? "INFO" : "ERROR")}] [UUID:{responseDTO.JobUUID}]");

                // 메타 정보 추가
                if (responseDTO.Meta != null)
                {
                    logBuilder.AppendLine($"Requester: {responseDTO.Meta.Requester ?? "N/A"}");
                    logBuilder.AppendLine($"Request URL: {responseDTO.Meta.RequestURL ?? "N/A"}");
                    logBuilder.AppendLine($"Request IP: {responseDTO.Meta.RequestIP ?? "N/A"}");
                    logBuilder.AppendLine($"Procedure Name: {responseDTO.Meta.Procedurename ?? "N/A"}");
                    logBuilder.AppendLine($"Row Count: {responseDTO.Meta.TableCount.ToString() ?? "N/A"}");
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

                // Deep Copy를 통해 ResponseDTO 복제 (Data 필드만 제외)
                // JSON 변환 후 다시 역직렬화하여 깊은 복사 수행
                string json = JsonConvert.SerializeObject(responseDTO);
                var logResponseDto = JsonConvert.DeserializeObject<ResponseDTO<object>>(json);

                // Data 필드만 대체
                logResponseDto.Data = "[데이터 필드 생략]";

                // JSON 직렬화 설정 (보기 좋게 포맷팅)
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };

                logBuilder.AppendLine($"Response DTO: {JsonConvert.SerializeObject(logResponseDto, jsonSettings)}");
                logBuilder.AppendLine(new string('-', 80)); // 구분선 추가

                // 6. 파일에 로그 저장 (파일 쓰기 동시성 제어)
                await WriteToFileWithRetryAsync(logFilePath, logBuilder.ToString());

                // 7. 로거에도 기록
                if (bIsSuccess)
                {
                    logger.LogInformation($"[API:{endpoint}] Job:{responseDTO.JobUUID} Time:{responseDTO.Meta.ExecutionTime} P.N:{responseDTO.ProcedureName} R.C:{responseDTO.TableCount} completed successfully");
                }
                else
                {
                    logger.LogError($"[API:{endpoint}] Job:{responseDTO.JobUUID} Time:{responseDTO.Meta.ExecutionTime} P.N:{responseDTO.ProcedureName} failed: {responseDTO.Message}");
                }
            }
            catch (Exception ex)
            {
                // 로그 저장 실패 시 로거에만 기록 (에러 전파 방지)
                logger.LogError(ex, "Failed to save log file");
            }
        }

        /// <summary>
        /// 파일에 로그를 쓰는 메소드 (재시도 로직 포함)
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="content">쓸 내용</param>
        private static async Task WriteToFileWithRetryAsync(string filePath, string content)
        {
            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // 파일 락 충돌을 방지하기 위한 FileShare 옵션 사용
                    using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                    {
                        await writer.WriteAsync(content);
                        await writer.FlushAsync();
                        return; // 성공적으로 쓰기 완료
                    }
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    // 다른 프로세스가 파일을 사용 중일 수 있으므로 잠시 대기 후 재시도
                    await Task.Delay(retryDelayMs * attempt);
                }
            }

            // 최대 재시도 횟수를 초과한 경우 마지막 시도
            await File.AppendAllTextAsync(filePath, content);
        }

        ///// <summary>
        ///// URL에서 엔드포인트 이름을 추출
        ///// </summary>
        //private static string ExtractEndpoint(string? url)
        //{
        //    if (string.IsNullOrEmpty(url))
        //    {
        //        return "unknown-endpoint";
        //    }

        //    // URL에서 마지막 경로 부분을 추출 (/api/controller/action -> action)
        //    string[] parts = url.TrimEnd('/').Split('/');
        //    string endpoint = parts.Length > 0 ? parts[^1] : "unknown-endpoint";

        //    // 쿼리 문자열 제거
        //    int queryIndex = endpoint.IndexOf('?');
        //    if (queryIndex > 0)
        //    {
        //        endpoint = endpoint.Substring(0, queryIndex);
        //    }

        //    // 컨트롤러 이름을 접두어로 추가
        //    if (parts.Length > 1)
        //    {
        //        string controller = parts[^2];
        //        return $"{controller}_{endpoint}";
        //    }

        //    return endpoint;
        //}


        
        /// <summary>
        /// URL 문자열에서 의미 있는 엔드포인트 식별자를 추출합니다.
        /// </summary>
        /// <param name="url">처리할 URL 문자열</param>
        /// <returns>식별된 엔드포인트 문자열(형식: "controller_action" 또는 특수 케이스)</returns>
        private static string ExtractEndpoint(string? url)
        {
            // 입력이 null이거나 빈 문자열인 경우 처리
            if (string.IsNullOrEmpty(url))
            {
                return "empty-endpoint";
            }

            // 루트 경로("/") 또는 공백만 있는 경우 처리
            url = url.Trim();
            if (url == "/" || url == string.Empty)
            {
                return "root-endpoint";
            }

            // URL 정규화 및 분석 준비
            string normalizedUrl = url.TrimEnd('/');

            // 쿼리 문자열 제거
            int queryIndex = normalizedUrl.IndexOf('?');
            if (queryIndex >= 0)
            {
                normalizedUrl = normalizedUrl.Substring(0, queryIndex);
            }

            // 경로 구성요소 분리
            string[] parts = normalizedUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 경로 구성요소가 없는 경우 처리 (예: "/", "//", "/?query=value")
            if (parts.Length == 0)
            {
                return "root-endpoint";
            }

            // 구성요소가 1개인 경우 (예: "/controller")
            if (parts.Length == 1)
            {
                return $"{parts[0]}_index";
            }

            // 일반적인 경우: 컨트롤러와 액션 결합 (예: "/controller/action")
            string controller = parts[parts.Length - 2];
            string action = parts[parts.Length - 1];

            // 특수 문자 제거 및 안전한 식별자 생성
            controller = CleanIdentifier(controller);
            action = CleanIdentifier(action);

            return $"{controller}_{action}";
        }

        /// <summary>
        /// 문자열에서 엔드포인트 식별자로 사용하기에 적합하지 않은 특수 문자를 제거합니다.
        /// </summary>
        /// <param name="input">정리할 식별자 문자열</param>
        /// <returns>정리된 식별자 문자열</returns>
        private static string CleanIdentifier(string input)
        {
            // 허용되지 않는 문자를 대시로 대체
            string pattern = @"[^a-zA-Z0-9_-]";
            return System.Text.RegularExpressions.Regex.Replace(input, pattern, "-");
        }

        /// <summary>
        /// 예외 유형에 따른 HTTP 상태 코드 반환
        /// </summary>
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