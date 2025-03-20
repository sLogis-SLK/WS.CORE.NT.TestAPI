using System.Text.RegularExpressions;
using Test_3TierAPI.CustomAnnotation;

namespace Test_3TierAPI.CustomAttribute.ValidAttribute
{
    public class FieldValidAttributes
    {
    }

    /// <summary>
    /// 단순 필드 존재 여부 검증 어트리뷰트
    /// 해당 작업에 대한 취소 작업은 RemoveFieldAttribute로 대체
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldRequireAttribute : FieldValidAttribute
    {
        public FieldRequireAttribute(string fieldName) : base(fieldName)
        {
        }

        protected override bool DoValidate(object value, out string errorMessage)
        {
            // 단순히 필드 존재 여부만 확인하는 어트리뷰트이므로 추가 검증 없음
            // 필드가 있고 null이 아니면 항상 통과
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// 문자열 길이 검증 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldLengthAttribute : FieldValidAttribute
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public FieldLengthAttribute(string fieldName, int minLength = 0, int maxLength = 0, bool isRequired = true)
          : base(fieldName, isRequired)
        {
            MinLength = minLength < 0 ? 0 : minLength;
            MaxLength = maxLength < 0 ? 0 : maxLength;
        }

        protected override bool DoValidate(object value, out string errorMessage)
        {
            errorMessage = string.Empty;

            string stringValue = value?.ToString() ?? string.Empty;

            if (MinLength > 0 && stringValue.Length < MinLength)
            {
                errorMessage = $"Field '{FieldName}' must be at least {MinLength} characters";
                return false;
            }

            if (MaxLength > 0 && stringValue.Length > MaxLength)
            {
                errorMessage = $"Field '{FieldName}' cannot exceed {MaxLength} characters";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 정규식 패턴 검증 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldPatternAttribute : FieldValidAttribute
    {
        public string Pattern { get; }
        public string CustomErrorMessage { get; }

        public FieldPatternAttribute(string fieldName, string pattern, string customErrorMessage = "", bool isRequired = true)
            : base(fieldName, isRequired)
        {
            Pattern = pattern;
            CustomErrorMessage = customErrorMessage;
        }

        protected override bool DoValidate(object value, out string errorMessage)
        {
            string stringValue = value?.ToString() ?? string.Empty;

            if (!Regex.IsMatch(stringValue, Pattern))
            {
                errorMessage = string.IsNullOrEmpty(CustomErrorMessage)
                    ? $"Field '{FieldName}' does not match the required pattern"
                    : CustomErrorMessage;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// 숫자 범위 검증 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldRangeAttribute : FieldValidAttribute
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public FieldRangeAttribute(string fieldName, double minValue = double.MinValue, double maxValue = double.MaxValue, bool isRequired = true)
            : base(fieldName, isRequired)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        protected override bool DoValidate(object value, out string errorMessage)
        {
            if (!TryConvertToDouble(value, out double numValue))
            {
                errorMessage = $"Field '{FieldName}' must be a valid number";
                return false;
            }

            if (numValue < MinValue)
            {
                errorMessage = $"Field '{FieldName}' must be greater than or equal to {MinValue}";
                return false;
            }

            if (numValue > MaxValue)
            {
                errorMessage = $"Field '{FieldName}' must be less than or equal to {MaxValue}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool TryConvertToDouble(object value, out double result)
        {
            result = 0;

            if (value == null)
                return false;

            if (value is double dbl)
            {
                result = dbl;
                return true;
            }

            if (value is int intVal)
            {
                result = intVal;
                return true;
            }

            if (value is decimal decVal)
            {
                result = (double)decVal;
                return true;
            }

            if (value is string strVal)
            {
                return double.TryParse(strVal, out result);
            }

            return false;
        }
    }

    /// <summary>
    /// 필드 값 일치 검증 어트리뷰트 (간소화 버전)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FieldEqualAttribute : FieldValidAttribute
    {
        public string TargetValue { get; }

        public FieldEqualAttribute(string fieldName, string targetValue, bool isRequired = true)
            : base(fieldName, isRequired)
        {
            TargetValue = targetValue;
        }

        protected override bool DoValidate(object value, out string errorMessage)
        {
            errorMessage = string.Empty;
            string strValue = (value ?? "").ToString();

            if (strValue != TargetValue)
            {
                errorMessage = $"Field '{FieldName}' must be equal to '{TargetValue}'";
                return false;
            }
            return true;
        }
    }
}
