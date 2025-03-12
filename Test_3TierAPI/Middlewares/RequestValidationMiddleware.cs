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
            // 요청 실행 시간 측정 시작
            Stopwatch? stopwatch = Stopwatch.StartNew();
            RequestDTO<object>? requestDto = null;
            string? body = null;

            // RateLimitMiddleware 에서 생성하고 context의 Items에 저장한 MetaDTO 가져오기
            MetaDTO? meta = context.Items.ContainsKey("MetaDTO") ? (MetaDTO)context.Items["MetaDTO"] : new MetaDTO();

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
                    await MiddlewareHelper.WriteErrorResponse(context, "Request body is empty", stopwatch, null);
                    return;
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError($"Error reading request body: {ex.Message}", ex);
                await MiddlewareHelper.WriteErrorResponse(context, "Error reading request body", stopwatch);
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
                await MiddlewareHelper.WriteErrorResponse(context, "Invalid request format (JSON parsing error).", stopwatch);
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
                            List<string> validationResults = MiddlewareHelper.ValidateRequest(requestDto, isDataRequired);
                            if (validationResults.Count > 0)
                            {
                                await MiddlewareHelper.WriteErrorResponse(context, "Invalid request data.", stopwatch, requestDto, validationResults);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 예외 발생 시 오류 메시지를 로깅하고 400 Bad Request 응답 반환
                    _logger.LogError($"Request validation failed: {ex.Message}");
                    await MiddlewareHelper.WriteErrorResponse(context, "Invalid request format.", stopwatch, requestDto);
                    return;
                }
            }

            // 기존 MetaDTO 사용 및 필요한 정보만 업데이트
            meta.RequestId = requestDto.RequestId;
            meta.TraceId = requestDto.TraceId;
            
            // 다시 context.Items에 저장
            context.Items["MetaDTO"] = meta;

            await _next(context);
        }
    }
}
