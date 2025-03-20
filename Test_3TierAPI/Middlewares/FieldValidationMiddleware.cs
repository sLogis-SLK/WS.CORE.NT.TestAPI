using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Test_3TierAPI.CustomAnnotation;
using Test_3TierAPI.CustomAttribute.ValidAttribute;
using Test_3TierAPI.Exceptions;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Middlewares
{
    /// <summary>
    /// 필드 유효성 검증 미들웨어
    /// 컨트롤러/액션에 적용된 어트리뷰트를 기반으로 Data 객체의 필드를 검증하고,
    /// 모든 검증 오류를 수집하여 한 번에 반환
    /// </summary>
    public class FieldValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FieldValidationMiddleware> _logger;

        public FieldValidationMiddleware(RequestDelegate next, ILogger<FieldValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // 1. Swagger 요청 등 무시할 경로인지 확인
            if (ShouldSkipValidation(context))
            {
                await _next(context);
                return;
            }

            // 2. 엔드포인트와 컨트롤러 액션 정보 가져오기
            var endpoint = context.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (actionDescriptor == null)
            {
                await _next(context);
                return;
            }

            // 3. 메서드 및 클래스 정보 가져오기
            var methodInfo = actionDescriptor.MethodInfo;
            var classType = methodInfo.DeclaringType;

            // 4. SkipValidation 어트리뷰트 확인 - 있으면 검증 스킵
            bool skipValidation = methodInfo.GetCustomAttribute<SkipFieldValidAttribute>() != null ||
                                  classType?.GetCustomAttribute<SkipFieldValidAttribute>() != null;

            if (skipValidation)
            {
                _logger.LogDebug($"Skipping validation for {methodInfo.Name} due to SkipValidationAttribute");
                await _next(context);
                return;
            }

            // 5. 필드 검증 어트리뷰트 수집
            var fieldValidators = CollectFieldValidators(methodInfo, classType);

            if (!fieldValidators.Any())
            {
                await _next(context);
                return;
            }

            // 6. 요청 본문 읽기
            string? requestBody = await ReadRequestBodyAsync(context);

            if (string.IsNullOrEmpty(requestBody))
            {
                await _next(context);
                return;
            }

            try
            {
                // 7. JSON 파싱
                JObject? jsonObj = JObject.Parse(requestBody);

                // 8. Data 객체 가져오기 및 검증
                JToken? dataObj = null;
                if (jsonObj.TryGetValue("data", StringComparison.OrdinalIgnoreCase, out JToken? data))
                {
                    dataObj = data;
                }

                List<string> fieldErrors = ValidateFields(dataObj, fieldValidators);

                // 9. 오류가 있으면 예외 발생
                if (fieldErrors.Any())
                {
                    throw new FieldValidationException($"Field validation failed: {string.Join(", ", fieldErrors)}");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse request JSON");
                throw new FieldValidationException($"Invalid JSON format: {ex.Message}");
            }
            catch (FieldValidationException)
            {
                // 이미 적절한 예외이므로 그대로 다시 throw
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation error");
                throw new FieldValidationException($"Validation error: {ex.Message}");
            }

            // 다음 미들웨어로 진행
            await _next(context);
        }

        /// <summary>
        /// 유효성 검증을 건너뛸지 여부 확인
        /// </summary>
        private bool ShouldSkipValidation(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger") ||
                   context.Request.Path.StartsWithSegments("/favicon.ico") ||
                   !context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                   !context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 필드 검증 어트리뷰트 수집 및 중복 처리
        /// 클래스와 메서드 레벨 어트리뷰트를 수집하고, 동일 필드에 대해 동일 타입의 어트리뷰트가 있으면
        /// 메서드 레벨 어트리뷰트로 대체함
        /// </summary>
        private Dictionary<string, List<IFieldValidAttribute>> CollectFieldValidators(MethodInfo methodInfo, Type classType)
        {
            var result = new Dictionary<string, List<IFieldValidAttribute>>(StringComparer.OrdinalIgnoreCase);

            // 1. 클래스 레벨 어트리뷰트 수집
            if (classType != null)
            {
                foreach (var attr in classType.GetCustomAttributes<FieldValidAttribute>(true))
                {
                    if (!result.ContainsKey(attr.FieldName))
                    {
                        result[attr.FieldName] = new List<IFieldValidAttribute>();
                    }

                    result[attr.FieldName].Add(attr);
                }
            }

            // 2. 메서드 레벨 어트리뷰트 수집 (같은 필드명의 같은 타입 어트리뷰트는 메서드 레벨로 대체)
            foreach (var attr in methodInfo.GetCustomAttributes<FieldValidAttribute>(true))
            {
                // 필드가 아직 등록되지 않은 경우 새로 추가
                if (!result.ContainsKey(attr.FieldName))
                {
                    result[attr.FieldName] = new List<IFieldValidAttribute>();
                    result[attr.FieldName].Add(attr);
                    continue;
                }

                // 동일 필드명에 동일 타입의 어트리뷰트가 있는지 확인
                var existingAttrs = result[attr.FieldName];
                var sameTypeAttr = existingAttrs.FirstOrDefault(a => a.GetType() == attr.GetType());

                if (sameTypeAttr != null)
                {
                    // 동일 타입의 어트리뷰트가 있는 경우, 기존 것을 제거하고 메서드 레벨 어트리뷰트로 대체
                    existingAttrs.Remove(sameTypeAttr);
                    existingAttrs.Add(attr);
                }
                else
                {
                    // 새 타입의 어트리뷰트는 그냥 추가
                    existingAttrs.Add(attr);
                }
            }

            // 3. RemoveFieldValidationAttribute 처리
            ProcessRemoveFieldAttributes(result, methodInfo, classType);

            return result;
        }

        /// <summary>
        /// RemoveFieldValidationAttribute가 적용된 필드의 모든 유효성 검증 어트리뷰트 제거
        /// </summary>
        private void ProcessRemoveFieldAttributes(Dictionary<string, List<IFieldValidAttribute>> validators,
            MethodInfo methodInfo, Type classType)
        {
            // 먼저 클래스 레벨의 RemoveFieldValidationAttribute 처리
            if (classType != null)
            {
                foreach (var attr in classType.GetCustomAttributes<RemoveFieldAttribute>(true))
                {
                    if (validators.ContainsKey(attr.FieldName))
                    {
                        validators.Remove(attr.FieldName);
                    }
                }
            }

            // 그다음 메서드 레벨의 RemoveFieldValidationAttribute 처리 (우선순위가 더 높음)
            foreach (var attr in methodInfo.GetCustomAttributes<RemoveFieldAttribute>(true))
            {
                if (validators.ContainsKey(attr.FieldName))
                {
                    validators.Remove(attr.FieldName);
                }
            }
        }

        /// <summary>
        /// HTTP 요청 본문 읽기
        /// </summary>
        private async Task<string?> ReadRequestBodyAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read request body");
                throw new FieldValidationException($"Failed to read request body: {ex.Message}");
            }
        }

        /// <summary>
        /// 필드 유효성 검증 수행 및 모든 오류 수집
        /// isRequired=false인 어트리뷰트는 해당 필드가 존재하지 않을 경우 검증을 건너뜀
        /// </summary>
        private List<string> ValidateFields(JToken? dataObj, Dictionary<string, List<IFieldValidAttribute>> fieldValidators)
        {
            var errors = new List<string>();

            // 필드 유효성 검사
            foreach (var fieldEntry in fieldValidators)
            {
                string fieldName = fieldEntry.Key;
                List<IFieldValidAttribute> attributeList = fieldEntry.Value;

                // 필수 필드 여부 확인
                bool isRequired = attributeList.Any(attr => attr.IsRequired);
                if (!isRequired)
                {
                    continue; // 필수 필드가 아니면 검사하지 않음
                }

                // dataObj가 없거나 null이면 필수 필드 검증
                if (dataObj == null || dataObj.Type == JTokenType.Null)
                {
                    errors.Add($"Field '{fieldName}' is required");
                    continue; // 필드 자체가 없으므로 이후 검증 불필요
                }

                // 형변환 없이 JToken에서 직접 SelectToken() 사용
                JToken? fieldValue = dataObj.SelectToken(fieldName);

                // 만약 fieldValue가 null이면 JSON 전체에서 해당 필드명 검색 (Descendants 사용)
                if (fieldValue == null)
                {
                    // SelectTokens를 사용하여 모든 속성을 검색
                    var properties = dataObj.SelectTokens("$..*")
                        .OfType<JProperty>()
                        .Where(p => string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));

                    // 일치하는 첫 번째 속성의 값 가져오기
                    fieldValue = properties.FirstOrDefault()?.Value;
                }

                if (fieldValue == null || fieldValue.Type == JTokenType.Null)
                {
                    errors.Add($"Field '{fieldName}' is required and cannot be null");
                    continue;
                }

                // 필드 값 가져오기(배열인지 확인 후 변환)
                object? valueObj;
                if (fieldValue.Type == JTokenType.Array)
                {
                    valueObj = fieldValue.ToObject<List<object>>(); // 배열이면 리스트로 변환
                }
                else if (fieldValue.Type == JTokenType.String)
                {
                    valueObj = fieldValue.ToString();
                }
                else
                {
                    valueObj = fieldValue.ToObject<object>();
                }


                // 필드 값 가져오기
                valueObj = fieldValue.Type == JTokenType.String
                    ? fieldValue.ToString()
                    : fieldValue.ToObject<object>();

                // 필드 값 검증
                foreach (var attr in attributeList)
                {
                    if (!attr.ValidateValue(valueObj, out string errorMessage) && !string.IsNullOrEmpty(errorMessage))
                    {
                        errors.Add(errorMessage);
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// JSON 객체에서 지정된 필드 이름에 해당하는 값을 검색합니다.
        /// 먼저 직접 경로로 시도하고, 실패할 경우 전체 JSON을 재귀적으로 검색합니다.
        /// </summary>
        /// <param name="jsonToken">검색할 JSON 객체</param>
        /// <param name="fieldName">찾을 필드 이름</param>
        /// <returns>찾은 필드의 값, 없으면 null</returns>
        public static JToken? FindFieldInJson(JToken jsonToken, string fieldName)
        {
            // 먼저 직접 SelectToken으로 시도
            JToken? fieldValue = jsonToken.SelectToken(fieldName);

            // SelectToken으로 찾지 못한 경우 전체 JSON에서 재귀적으로 검색
            if (fieldValue == null)
            {
                fieldValue = FindPropertyRecursively(jsonToken, fieldName);
            }

            return fieldValue;
        }

        /// <summary>
        /// JSON 객체 내에서 지정된 속성 이름을 재귀적으로 검색합니다.
        /// 모든 중첩 수준을 검색하며 객체와 배열을 모두 고려합니다.
        /// </summary>
        /// <param name="token">검색할 JSON 토큰</param>
        /// <param name="propName">찾을 속성 이름</param>
        /// <returns>찾은 속성의 값, 없으면 null</returns>
        private static JToken? FindPropertyRecursively(JToken token, string propName)
        {
            // 현재 토큰이 JProperty이고 이름이 일치하는 경우
            if (token is JProperty prop &&
                string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase))
            {
                return prop.Value;
            }

            // 현재 토큰이 JObject인 경우 (객체)
            if (token is JObject obj)
            {
                // 직접 속성 확인
                JProperty directProp = obj.Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));

                if (directProp != null)
                    return directProp.Value;

                // 모든 하위 속성 확인
                foreach (JProperty property in obj.Properties())
                {
                    JToken result = FindPropertyRecursively(property.Value, propName);
                    if (result != null)
                        return result;
                }
            }

            // 현재 토큰이 JArray인 경우 (배열)
            if (token is JArray array)
            {
                foreach (JToken item in array)
                {
                    JToken result = FindPropertyRecursively(item, propName);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

    }
        ///// <summary>
        ///// 미들웨어 확장 메서드
        ///// </summary>
        //public static class FieldValidationMiddlewareExtensions
        //{
        //    /// <summary>
        //    /// 필드 유효성 검증 미들웨어 등록
        //    /// </summary>
        //    public static IApplicationBuilder UseFieldValidation(this IApplicationBuilder builder)
        //    {
        //        return builder.UseMiddleware<FieldValidationMiddleware>();
        //    }
        //}
}