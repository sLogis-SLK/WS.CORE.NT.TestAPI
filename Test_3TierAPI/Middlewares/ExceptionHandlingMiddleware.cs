using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            // JobUUID 및 MetaDTO 초기화 (항상 새로 생성)
            bool bIsDev = _env.IsDevelopment();
            var meta = new MetaDTO
            {
                ServerTimeStamp = DateTime.Now,
                RequestIP = context.Connection.RemoteIpAddress?.ToString(),
                RequestURL = context.Request.Path
            };

            // 각 요청별로 다른 jobUUID 생성
            string? jobUUID = meta.ServerTimeStamp?.ToString("yyyy-MM-dd_HH:mm:ss") + "_" + meta.RequestIP + "_" + Guid.NewGuid().ToString();
            meta.JobUUID = jobUUID;

            // Context에 저장
            context.Items["MetaDTO"] = meta;
            context.Items["JobUUID"] = jobUUID;
            context.Items["bIsDev"] = bIsDev;

            // 요청 시간 측정을 위한 Stopwatch 시작
            var stopwatch = Stopwatch.StartNew();
            context.Items["Stopwatch"] = stopwatch;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 실행 시간 측정 완료
                stopwatch.Stop();

                // 최신 MetaDTO 가져오기 (다른 미들웨어에서 업데이트 한 값 반영)
                context.Items.TryGetValue("MetaDTO", out var metaObj);
                meta = metaObj as MetaDTO ?? meta;
                
                // 가장 처음의 예외 가져오기
                Exception innerEx = ex;
                while (innerEx.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }

                // http 상태 코드 설정
                int statusCode = MiddlewareHelper.GetStatusCode(innerEx);
                meta.StatusCode = statusCode;
                meta.ExecutionTime = $"{stopwatch.ElapsedMilliseconds}ms";
                context.Response.StatusCode = statusCode;

                
                // meta.ErrorDetail = innerEx.Message;
                string errorMessage = $"Unhandled Error: {innerEx.Message}";
                // meta에는 StackTrace까지 저장
                meta.ErrorDetail = errorMessage + "\n" + innerEx.StackTrace;

                // `ResponseDTO` 객체를 먼저 생성 (모든 정보 포함)
                var errorResponseDTO = new ResponseDTO<object>
                {
                    JobUUID = meta.JobUUID,
                    Success = false,
                    StatusCode = statusCode,
                    Message = errorMessage,
                    Meta = meta // 로그 저장을 위해 모든 정보를 포함
                };

                MiddlewareHelper.SaveLogToFile(_logger, errorResponseDTO, errorResponseDTO.Success);
                
                // 개발 환경 여부에 따른 Respone 가공
                if(!bIsDev)
                {
                    errorResponseDTO.Message = "An unexpected error occured.";
                    errorResponseDTO.Meta = null;   // 운영환경이 아니면 metaDTO 제거
                }

                await context.Response.WriteAsJsonAsync(errorResponseDTO);
            }
        }
    }
}
