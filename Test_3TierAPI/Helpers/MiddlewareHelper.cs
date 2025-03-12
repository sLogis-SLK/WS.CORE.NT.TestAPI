using System.Diagnostics;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Helpers
{
    public static class MiddlewareHelper
    {
        /// <summary>
        /// 요청 DTO를 검증하여 유효성 검사 오류 목록을 반환
        /// </summary>
        public static List<string> ValidateRequest(RequestDTO<object> request, bool isDataRequired)
        {
            var errors = new List<string>();

            // 요청 객체가 null이면 에러 반환
            if (request == null)
            {
                errors.Add("Request body cannot be null");
                return errors;
            }

            // 필수 필드 검증 (Requester, RequestId, RequestTimestamp)
            if (string.IsNullOrWhiteSpace(request.Requester))
            {
                errors.Add("Requester is required");
            }
            if (string.IsNullOrWhiteSpace(request.RequestId))
            {
                errors.Add("RequestId is required");
            }
            if (request.RequestTimestamp == default || request.RequestTimestamp > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("RequestTimestamp must be a valid past or present timestamp.");
            }
            if (!string.IsNullOrWhiteSpace(request.TraceId) && !Guid.TryParse(request.TraceId, out _))
            {
                errors.Add("TraceId must be a valid GUID.");
            }

            // RequireData 어노테이션이 있고, Data가 필수인데 없으면 오류 추가
            if (isDataRequired && request.Data == null)
            {
                errors.Add("Data is required for this endpoint.");
            }

            return errors;
        }

        /// <summary>
        /// 에러 응답을 JSON 형식으로 반환하는 메서드
        /// </summary>
        public static void StoreErrorResponse(ResponseDTO<object> errorResponse, HttpContext context)
        {
            // 응답 객체를 context의 items에 저장
            context.Items["ErrorResponseDTO"] = errorResponse;
        }

        public static ResponseDTO<object> GetErrorResponse<T>(
            HttpContext context, string message, bool bIsDev, Stopwatch stopwatch, MetaDTO metaDTO,
            List<string>? details = null)
        {
            stopwatch.Stop(); // 요청 처리 종료 시점 측정
            metaDTO.ExecutionTime = $"{stopwatch.ElapsedMilliseconds}ms"; // 실행 시간 기록

            // 개발환경과, details가 있는 경우에 대한 메시지 처리
            string returnMessage = bIsDev ? message : "An error occurred while processing your request.";
            returnMessage = details != null ? $"{returnMessage}\nDetails: {string.Join(", ", details)}" : returnMessage;

            // 에러 응답 객체 생성
            var errorResponse = new ResponseDTO<object>
            {
                JobUUID = metaDTO.JobUUID,
                Success = false,
                StatusCode = metaDTO.StatusCode,            // 각 컨트롤러에서 MetaDTO에 적절한 StatusCode할당 할 예정
                Message = returnMessage,
                Data = null,
            };

            if (bIsDev) errorResponse.Meta = metaDTO;

            // 응답 객체를 context의 items에 저장
            return errorResponse;
        }

        public static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
