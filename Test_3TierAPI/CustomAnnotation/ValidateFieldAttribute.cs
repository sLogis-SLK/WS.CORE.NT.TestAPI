using System;

namespace Test_3TierAPI.CustomAnnotation
{
    /// <summary>
    /// 필드 유효성 검증을 위한 커스텀 어트리뷰트
    /// 컨트롤러 클래스나 액션 메서드에 적용하여 RequestDTO의 Data 필드 검증
    /// 여러 개의 필드를 검증하기 위해 여러 번 사용 가능
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ValidateFieldAttribute : Attribute
    {
        /// <summary>
        /// 검증할 필드 이름
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 필수 필드 여부
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 최대 길이 (문자열 필드에 적용)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// 최소 길이 (문자열 필드에 적용)
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// 정규식 패턴 (문자열 필드에 적용)
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// 최소값 (숫자 필드에 적용)
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// 최대값 (숫자 필드에 적용)
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// 다른 필드와의 동일성 검사 (같은 값이어야 함)
        /// </summary>
        public string? EqualTo { get; set; }

        /// <summary>
        /// 필드 유효성 검증 어트리뷰트 생성자
        /// </summary>
        /// <param name="fieldName">검증할 필드 이름</param>
        /// <param name="required">필수 필드 여부</param>
        public ValidateFieldAttribute(string fieldName, bool required = false)
        {
            FieldName = fieldName;
            Required = required;
        }

        /// <summary>
        /// 문자열 길이 제한 설정
        /// </summary>
        /// <param name="minLength">최소 길이</param>
        /// <param name="maxLength">최대 길이</param>
        /// <returns>현재 어트리뷰트 인스턴스</returns>
        public ValidateFieldAttribute WithLength(int minLength = 0, int maxLength = int.MaxValue)
        {
            MinLength = minLength > 0 ? minLength : null;
            MaxLength = maxLength < int.MaxValue ? maxLength : null;
            return this;
        }

        /// <summary>
        /// 정규식 패턴 설정
        /// </summary>
        /// <param name="pattern">정규식 패턴</param>
        /// <returns>현재 어트리뷰트 인스턴스</returns>
        public ValidateFieldAttribute WithPattern(string pattern)
        {
            Pattern = pattern;
            return this;
        }

        /// <summary>
        /// 숫자 범위 설정
        /// </summary>
        /// <param name="minValue">최소값</param>
        /// <param name="maxValue">최대값</param>
        /// <returns>현재 어트리뷰트 인스턴스</returns>
        public ValidateFieldAttribute WithRange(double minValue = double.MinValue, double maxValue = double.MaxValue)
        {
            MinValue = minValue > double.MinValue ? minValue : null;
            MaxValue = maxValue < double.MaxValue ? maxValue : null;
            return this;
        }

        /// <summary>
        /// 필드 동일성 검사 설정
        /// </summary>
        /// <param name="otherFieldName">비교할 다른 필드 이름</param>
        /// <returns>현재 어트리뷰트 인스턴스</returns>
        public ValidateFieldAttribute EqualToField(string otherFieldName)
        {
            EqualTo = otherFieldName;
            return this;
        }
    }
}
