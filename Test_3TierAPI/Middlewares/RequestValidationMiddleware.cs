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

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // 0. Swagger 요청은 Body 검사를 하지 않음
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }
            // 1. Context에서 필요한 정보를 가져옴 (MetaDTO, Stopwatch, bIsDev)
            // 튜플 분해(Tuple Deconstruction) 문법 사용
            (MetaDTO meta, Stopwatch stopwatch, bool bIsDev) = GetContextMetadata(context);

            // 2. 요청 본문을 읽음 (읽기 실패 시 종료)
            string? body = await ExtractRequestBody(context, meta, stopwatch, bIsDev);
            if (body == null) return;

            // 3. JSON 역직렬화 (파싱 실패 시 종료)
            RequestDTO<object>? requestDto = ParseJsonRequest(body, context, meta, stopwatch, bIsDev);
            if (requestDto == null) return;

            // 4. HTTP 메서드가 POST 또는 PUT일 경우 데이터 검증 실행
            bool isPostOrPut = context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put;
            if (isPostOrPut)
            {
                bool isValid = ValidateRequestData(context, requestDto, meta, stopwatch, bIsDev);
                if (!isValid) return;
            }

            // 5. MetaDTO 정보 업데이트
            UpdateMetaDTO(context, requestDto, meta);

            // 6. 다음 미들웨어로 요청 전달
            await _next(context);
        }

        /// <summary>
        /// Context에서 기존 Middleware에서 저장한 MetaDTO, Stopwatch, bIsDev 값을 가져옴.
        /// 존재하지 않으면 기본값을 생성하여 반환.
        /// </summary>
        private (MetaDTO, Stopwatch, bool) GetContextMetadata(HttpContext context)
        {
            // Context.Items에서 기존 데이터를 가져옴 (없으면 기본값 할당)
            context.Items.TryGetValue("MetaDTO", out object? metaObj);
            context.Items.TryGetValue("Stopwatch", out object? stopwatchObj);
            context.Items.TryGetValue("bIsDev", out object? bIsDevObj);

            MetaDTO meta = metaObj as MetaDTO ?? new MetaDTO();
            Stopwatch stopwatch = stopwatchObj as Stopwatch ?? new Stopwatch();
            bool bIsDev = bIsDevObj is bool devMode && devMode;

            return (meta, stopwatch, bIsDev);
        }

        /// <summary>
        /// 요청 본문을 읽어 문자열로 반환.
        /// 본문이 없거나 읽기 오류 발생 시, 에러 응답을 생성하고 null 반환.
        /// </summary>
        private async Task<string?> ExtractRequestBody(HttpContext context, MetaDTO meta, Stopwatch stopwatch, bool bIsDev)
        {
            try
            {
                // HTTP 요청 본문을 여러 번 읽을 수 있도록 버퍼링 활성화
                context.Request.EnableBuffering();

                // StreamReader를 사용하여 요청 본문을 읽음
                using StreamReader reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();

                // 스트림 위치를 0으로 초기화하여 다른 미들웨어에서도 본문을 읽을 수 있도록 함
                context.Request.Body.Position = 0;

                // 요청 본문이 비어 있으면 예외 발생
                if (string.IsNullOrWhiteSpace(body))
                {
                    throw new InvalidOperationException("Request body is empty");
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body");
                HandleValidationFailure(context, "Error reading request body", meta, stopwatch, bIsDev, StatusCodes.Status400BadRequest);
                return null;
            }
        }

        /// <summary>
        /// JSON 문자열을 RequestDTO 객체로 변환.
        /// JSON 파싱 오류 발생 시 에러 응답 반환.
        /// </summary>
        private RequestDTO<object>? ParseJsonRequest(string body, HttpContext context, MetaDTO meta, Stopwatch stopwatch, bool bIsDev)
        {
            try
            {
                return JsonSerializer.Deserialize<RequestDTO<object>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error: {Body}", body);
                HandleValidationFailure(context, "Invalid request format (JSON parsing error).", meta, stopwatch, bIsDev, StatusCodes.Status400BadRequest);
                return null;
            }
        }

        /// <summary>
        /// 요청 데이터를 검증하고, 유효하지 않으면 에러 응답 반환.
        /// </summary>
        private bool ValidateRequestData(HttpContext context, RequestDTO<object> requestDto, MetaDTO meta, Stopwatch stopwatch, bool bIsDev)
        {
            try
            {
                // API 엔드포인트의 컨트롤러 및 메서드 정보를 가져옴
                Endpoint? endpoint = context.GetEndpoint();
                ControllerActionDescriptor? actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                MethodInfo? methodInfo = actionDescriptor?.MethodInfo;
                Type? classType = methodInfo?.DeclaringType;

                // 컨트롤러 또는 메서드에 RequireDataAttribute가 있는지 확인
                bool isDataRequired = methodInfo?.GetCustomAttribute<RequireDataAttribute>()?.IsRequired ??
                                      classType?.GetCustomAttribute<RequireDataAttribute>()?.IsRequired ??
                                      false;

                // 요청 DTO 검증 실행
                List<string> validationResults = ValidateRequest(requestDto, isDataRequired);
                if (validationResults.Count > 0)
                {
                    _logger.LogWarning("Invalid request data: {Errors}", string.Join(", ", validationResults));
                    HandleValidationFailure(context, "Invalid request data.", meta, stopwatch, bIsDev, StatusCodes.Status400BadRequest, validationResults);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request validation failed");
                HandleValidationFailure(context, "Invalid request format.", meta, stopwatch, bIsDev, StatusCodes.Status400BadRequest);
                return false;
            }
        }

        /// <summary>
        /// MetaDTO 정보를 업데이트하여 요청자 및 타임스탬프 추가.
        /// </summary>
        private void UpdateMetaDTO(HttpContext context, RequestDTO<object> requestDto, MetaDTO meta)
        {
            meta.Requester = requestDto.Requester;
            meta.SWRequestTimestamp = requestDto.RequestTimestamp;
            context.Items["MetaDTO"] = meta;
        }

        /// <summary>
        /// 요청 DTO의 유효성을 검사하여 실패 메시지 리스트를 반환.
        /// </summary>
        private List<string> ValidateRequest(RequestDTO<object>? request, bool isDataRequired)
        {
            List<string> errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request body cannot be null");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.Requester))
            {
                errors.Add("Requester is required");
            }

            if (request.RequestTimestamp == default || request.RequestTimestamp > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("RequestTimestamp must be a valid past or present timestamp.");
            }

            if (isDataRequired && request.Data == null)
            {
                errors.Add("Data is required for this endpoint.");
            }

            return errors;
        }

        /// <summary>
        /// 검증 실패 시 공통적인 에러 처리 로직.
        /// </summary>
        private void HandleValidationFailure(HttpContext context, string message, MetaDTO meta, Stopwatch stopwatch, bool bIsDev, int statusCode, List<string>? errors = null)
        {
            meta.StatusCode = statusCode;
            MiddlewareHelper.StoreErrorResponse(MiddlewareHelper.GetErrorResponse<object>(context, message, bIsDev, stopwatch, meta, errors), context);
        }
    }
}
