using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Test_3TierAPI.CustomAttribute;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    /// <summary>
    /// RequestValidationMiddleware
    /// RequestDTO의 유효성을 검사하는 미들웨어
    /// [RequireData] 커스텀 어트리뷰트를 사용하여 요청 데이터의 필수 여부를 지정 및 확인
    /// [RequireData(true)] 인 컨트롤러 클래스 또는 Endpoint 함수에 대해서만 RequestDTO의 Data가 필수로 요구됨
    /// </summary>
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
            string? body = await ExtractRequestBody(context);
            if (body == null) throw new InvalidOperationException("[RequestValidMiddleware] Request body is empty");

            // 3. JSON 역직렬화 (파싱 실패 시 종료)
            RequestDTO<object>? requestDto = ParseJsonRequest(body);
            if (requestDto == null) throw new InvalidOperationException("[RequestValidMiddleware] RequestDTO is null");

            // 4. HTTP 메서드가 POST 또는 PUT일 경우 데이터 검증 실행
            bool isPostOrPut = context.Request.Method == HttpMethods.Post;
            if (isPostOrPut)
            {
                bool isValid = ValidateRequestData(context, requestDto);
                if (!isValid) throw new Exception("[RequestValidMiddleware] Invalid request data");
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
        /// TODO
        ///     - 이 부분은 추후 정확한 로직 공부 필요
        /// </summary>
        private async Task<string?> ExtractRequestBody(HttpContext context)
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
                    throw new InvalidOperationException("[RequestValidMiddleware] Request body is empty");
                }

                return body;
            }
            catch (Exception ex)
            {
                throw new Exception("[RequestValidMiddleware] Failed to read request body : " + ex.Message);
            }
        }

        /// <summary>
        /// JSON 문자열을 RequestDTO 객체로 변환.
        /// JSON 파싱 오류 발생 시 에러 응답 반환.
        /// </summary>
        private RequestDTO<object>? ParseJsonRequest(string body)
        {
            try
            {
                return JsonSerializer.Deserialize<RequestDTO<object>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new JsonException("[RequestValidMiddleware]Failed to parse JSON request: " + ex.Message);
            }
        }

        /// <summary>
        /// 요청 데이터를 검증하고, 유효하지 않으면 에러 응답 반환.
        /// API 요청을 받은 컨트롤러 또는 Endpoint의 메서드에서 RequireDataAttribute 값을 받아와서
        /// Data가 필요한지 여부를 확인하여 검증.
        /// </summary>
        private bool ValidateRequestData(HttpContext context, RequestDTO<object> requestDto)
        {
            try
            {
                // 현재 HTTP 요청과 연결된 엔드포인트 정보를 가져옴
                // ASP.NET Core의 라우팅 정보를 포함한 Endpoint 객체
                Endpoint? endpoint = context.GetEndpoint();

                // 해당 엔드포인트에서 실행될 컨트롤러 및 메서드 정보를 가져옴
                ControllerActionDescriptor? actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

                // 액션 메서드의 정보를 가져옴 (MethodInfo 객체)
                MethodInfo? methodInfo = actionDescriptor?.MethodInfo;

                // 액션이 속한 컨트롤러의 타입을 가져옴 (Type 객체)
                Type? classType = methodInfo?.DeclaringType;

                // 컨트롤러 또는 액션 메서드에 RequireDataAttribute가 있는지 확인
                // RequireDataAttribute가 true인 경우, 요청 데이터가 반드시 포함되어야 함
                bool isDataRequired = methodInfo?.GetCustomAttribute<RequireDataAttribute>()?.IsRequired ??
                                      classType?.GetCustomAttribute<RequireDataAttribute>()?.IsRequired ??
                                      false;

                // 요청 데이터(requestDto)의 유효성 검사 실행
                List<string> validationResults = ValidateRequest(requestDto, isDataRequired);

                // 유효성 검사 실패 시 예외 발생
                if (validationResults.Count > 0)
                {
                    // 에러 메시지를 쉼표로 연결하여 반환
                    throw new Exception("[RequestValidMiddleware] Invalid request data: " + string.Join(", ", validationResults));
                }

                // 검증이 통과된 경우 true 반환
                return true;
            }
            catch (Exception ex)
            {
                // 유효성 검사 중 오류 발생 시, 예외를 감싸서 다시 던짐
                throw new Exception("[RequestValidMiddleware] Failed to validate request data: ", ex);
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

            
            // 1. RequestTimestamp가 기본값 (0001-01-01 00:00:00)인지 확인
            if (request.RequestTimestamp == default)
            {
                errors.Add("RequestTimestamp is required and cannot be empty.");
            }
            // 2. RequestTimestamp가 현재 서버 시간보다 5분 이상 미래인지 확인
            else if (request.RequestTimestamp > DateTime.Now.AddMinutes(5))
            {
                errors.Add("RequestTimestamp cannot be more than 5 minutes in the future.");
            }

            if (isDataRequired && request.Data == null)
            {
                errors.Add("Data is required for this endpoint.");
            }

            return errors;
        }
    }
}
