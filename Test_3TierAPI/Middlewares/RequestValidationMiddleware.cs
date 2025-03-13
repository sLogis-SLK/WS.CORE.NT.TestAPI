using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Test_3TierAPI.CustomAnnotation;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;

        // 요청 처리를 위한 다음 Delegate(_next)와 로깅(_logger)을 주입받음
        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Middleware의 핵심 기능을 수행하는 Invoke 메서드
        public async Task Invoke(HttpContext context)
        {
            // exception middleware에서 생성한 items들 가져오기
            Stopwatch? stopwatch = context.Items.ContainsKey("Stopwatch") ? context.Items["Stopwatch"] as Stopwatch : new Stopwatch();
            bool bIsDev = context.Items.ContainsKey("bIsDev") && (bool)context.Items["bIsDev"];
            MetaDTO? meta = context.Items.ContainsKey("MetaDTO") ? context.Items["MetaDTO"] as MetaDTO : new MetaDTO();

            RequestDTO<object>? requestDto = null;
            string? body = null;

            try
            {
                // 요청 본문을 다시 읽을 수 있도록 버퍼링 활성화
                // 기본적으로 .net의 HttpRequest.Body는 한 번만 읽을 수 있으므로 이를 다시 읽을 수 있도록 함
                context.Request.EnableBuffering();

                // 요청 본문을 읽고 Json을 문자열로 변환
                using var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, true, 1024, true);
                body = await reader.ReadToEndAsync();

                // 스트림 위치를 초기화하여 이후 컨트롤러에서 요청 본문을 다시 읽을 수 있도록 함
                context.Request.Body.Position = 0;

                // 요청 본문이 비어 있는 경우 에러 응답 반환
                if (string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogWarning("Request body is empty");
                    meta.StatusCode = StatusCodes.Status400BadRequest;
                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Request body is empty", bIsDev, stopwatch, meta), context);
                    return;
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError($"Error reading request body: {ex.Message}", ex);
                meta.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Error reading request body", bIsDev, stopwatch, meta), context);
                return;
            }

            try
            {
                // JSON 역직렬화에서 예외가 발생할 가능성이 높음
                requestDto = JsonSerializer.Deserialize<RequestDTO<object>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}", ex);
                meta.StatusCode = StatusCodes.Status500InternalServerError;
                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Invalid request format (JSON parsing error).", bIsDev, stopwatch, meta), context);
                return;
            }

            // http 메서드가 post 또는 put일 경우에만 검증 실행
            // - GET, DELETE 등의 요청에서는 RequestDTO가 필요하지 않음
            if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
            {
                try
                {
                    // 현재 요청된 API 엔드포인트 가져오기
                    Endpoint? endpoint = context.GetEndpoint();
                    if(endpoint != null)
                    {
                        // 컨트롤러 액션 메서드의 메타데이터를 가져옴
                        ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                        if(actionDescriptor != null)
                        {
                            // 해당 컨트롤러의 실제 메서드 정보를 가져옴
                            MethodInfo methodInfo = actionDescriptor.MethodInfo;
                            Type? classType = methodInfo.DeclaringType;   // 컨트롤러 클래스 정보 가져오기

                            // 메서드와 컨트롤러의 RequireDataAttribute 가져오기
                            RequireDataAttribute? methodRequireDataAttr = methodInfo.GetCustomAttribute<RequireDataAttribute>();
                            RequireDataAttribute? classRequireDataAttr = classType?.GetCustomAttribute<RequireDataAttribute>();

                            // 최종적으로 적용할 RequireData 어노테이션(Attribute) 가져오기
                            bool isDataRequired = methodRequireDataAttr?.IsRequired ?? classRequireDataAttr?.IsRequired ?? false;

                            // 요청 DTO의 유효성을 검사하고, 실패한 경우 에러 응답 반환
                            List<string> validationResults = ValidateRequest(requestDto, isDataRequired);
                            if (validationResults.Count > 0)
                            {
                                _logger.LogWarning("Invalid request data: {0}", string.Join(", ", validationResults));
                                meta.StatusCode = StatusCodes.Status400BadRequest;
                                MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Invalid request data.", bIsDev, stopwatch, meta, validationResults), context);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 예외 발생 시 오류 메시지를 로깅하고 400 Bad Request 응답 반환
                    _logger.LogError($"Request validation failed: {ex.Message}");
                    meta.StatusCode = StatusCodes.Status400BadRequest;
                    MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, "Invalid request format.", bIsDev, stopwatch, meta), context);
                    return;
                }
            }

            // 기존 MetaDTO 사용 및 필요한 정보만 업데이트
            meta.Requester = requestDto.Requester;
            meta.SWRequestTimestamp = requestDto.RequestTimestamp;


            // 다시 context.Items에 저장
            context.Items["MetaDTO"] = meta;

            await _next(context);
        }

        /// <summary>
        /// 요청 DTO를 검증하여 유효성 검사 오류 목록을 반환
        /// </summary>
        public List<string> ValidateRequest(RequestDTO<object> request, bool isDataRequired)
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
            if (request.RequestTimestamp == default || request.RequestTimestamp > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("RequestTimestamp must be a valid past or present timestamp.");
            }
            // RequireData 어노테이션이 있고, Data가 필수인데 없으면 오류 추가
            if (isDataRequired && request.Data == null)
            {
                errors.Add("Data is required for this endpoint.");
            }

            return errors;
        }
    }
}
