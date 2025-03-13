using System.Diagnostics;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Helpers
{
    public static class MiddlewareHelper
    {
        

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
