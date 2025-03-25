using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using System.Diagnostics;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.ActionFilters
{
    public class ApiResponseFilter : IActionFilter
    {
        private readonly ILogger<ApiResponseFilter> _logger;
        private readonly IHostEnvironment _env;


        public ApiResponseFilter(ILogger<ApiResponseFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Controller 실행 전 수행되어야 할 로직 (현재 필요 없음)
        }

        /// <summary>
        /// 컨트롤러 응답 데이터를 ResponseDTO로 변환 (자동으로 변환)
        /// 내부적으로 Newtonsoft.Json을 사용하여 DataTable도 자동으로 변환(Program.cs에 AddControllers().AddNewtonsoftJson() 추가 되어 있음)
        /// 이 클래스에는 Exception 구현하면 안됨 -> ExceptionMiddleware로 예외 처리 하는 로직에 문제 생김
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 이미 오류가 발생한 경우 압축을 시도하지 않음
            if (context.Exception != null) return;

            // 1️. Stopwatch 가져오기 (없으면 로그만 남기고 진행)
            if (!context.HttpContext.Items.TryGetValue("Stopwatch", out var stopwatchObj) || stopwatchObj is not Stopwatch stopwatch)
            {
                _logger.LogWarning("[ApiResponseFilter] Stopwatch not found in HttpContext.Items.");
                stopwatch = new Stopwatch(); // 기본값 설정
            }
            stopwatch.Stop();

            // 2️. MetaDTO 가져오기 (없으면 기본값 할당)
            if (!context.HttpContext.Items.TryGetValue("MetaDTO", out var metaObj) || metaObj is not MetaDTO meta)
            {
                _logger.LogWarning("[ApiResponseFilter] MetaDTO is missing in HttpContext.Items. Creating a default instance.");
                meta = new MetaDTO
                {
                    ServerTimeStamp = DateTime.UtcNow,
                    RequestIP = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    RequestURL = context.HttpContext.Request.Path,
                    JobUUID = Guid.NewGuid().ToString()
                };
            }

            // ProcedureName 가져오기
            if (!context.HttpContext.Items.TryGetValue("ProcedureName", out var procNameObj) || procNameObj is not string procName)
            {
                _logger.LogWarning("[ApiResponseFilter] ProcedureName is missing in HttpContext.Items. Setting to 'N/A'.");
                procName = "N/A";
            }

            // 최종 실행시간 체크
            meta.ExecutionTime = $"{stopwatch.ElapsedMilliseconds}ms";

            // 3️. context.Result가 ObjectResult인지 확인
            if (context.Result is not ObjectResult result)
            {
                _logger.LogWarning("[ApiResponseFilter] Result is not ObjectResult. Creating a default response.");
                result = new ObjectResult(null)
                {
                    StatusCode = context.HttpContext.Response.StatusCode
                };
            }

            // 컨트롤러 응답 데이터(DB 응답 데이터)
            var responseData = result.Value ?? new object(); // 기본값 설정

            // 4️. ResponseDTO 생성
            var response = new ResponseDTO<object>
            {
                JobUUID = meta.JobUUID,
                Success = true,
                Message = "요청이 정상적으로 처리되었습니다.",
                Data = responseData,
                ProcedureName = procName,
                TableCount = 0,
                Meta = meta
            };

            if (response.Data is DataTable table) response.TableCount = response.Meta.TableCount = table.Rows.Count;

            response.StatusCode = response.Meta.StatusCode = context.HttpContext.Response.StatusCode;

            // 5️. ResponseDTO를 HttpContext.Items에 저장 (로깅 미들웨어에서 사용)
            context.HttpContext.Items["ResponseDTO"] = response;

            // 6. 성공시 로그 저장
            Task logTask = MiddlewareHelper.SaveLogToFileAsync(_logger, response, true);
            
            logTask.Wait(); // 로그 저장 완료까지 대기

            //// 67. 운영 환경에서는 MetaDTO를 숨김
            //if (!_env.IsDevelopment())
            //{
            //    response.Meta = null;
            //}

            // 8. 최종 응답을 ObjectResult로 변경
            context.Result = new ObjectResult(response);    // 응답 response를 깊은 복사함. 그래서 로깅 미들웨어에서 Data를 null로 만들어도 문제가 없음
        }
    }
}
