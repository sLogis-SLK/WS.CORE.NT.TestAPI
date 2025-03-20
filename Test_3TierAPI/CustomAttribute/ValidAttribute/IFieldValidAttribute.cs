using System;

namespace Test_3TierAPI.CustomAnnotation
{
    /// <summary>
    /// 필드 검증 어트리뷰트를 위한 인터페이스
    /// </summary>
    public interface IFieldValidAttribute
    {
        /// <summary>
        /// 검증할 필드 이름
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// 필수 필드 여부 - false면 이 필드가 없어도 오류가 발생하지 않음
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// 필드 값 검증 메서드
        /// </summary>
        /// <param name="value">검증할 필드 값</param>
        /// <param name="errorMessage">검증 실패 시 오류 메시지</param>
        /// <returns>검증 성공 여부</returns>
        bool ValidateValue(object value, out string errorMessage);
    }

    /// <summary>
    /// 필드 검증 어트리뷰트의 기본 구현을 제공하는 추상 클래스
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class FieldValidAttribute : Attribute, IFieldValidAttribute
    {
        /// <summary>
        /// 검증할 필드 이름
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// 필수 필드 여부
        /// true인 경우 필드가 반드시 있어야 함
        /// false인 경우 필드가 없어도 오류가 발생하지 않음
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        /// <param name="fieldName">검증할 필드 이름</param>
        /// <param name="isRequired">필수 필드 여부 (기본값: true)</param>
        protected FieldValidAttribute(string fieldName, bool isRequired = true)
        {
            FieldName = fieldName;
            IsRequired = isRequired;
        }

        /// <summary>
        /// 필드 값 검증 메서드
        /// </summary>
        public bool ValidateValue(object value, out string errorMessage)
        {
            // null 값 검증은 미들웨어에서 별도로 처리하므로 여기서는 구체적인 값 검증만 수행
            return DoValidate(value, out errorMessage);
        }

        /// <summary>
        /// 파생 클래스에서 구현할 구체적인 필드 값 검증 메서드
        /// </summary>
        protected abstract bool DoValidate(object value, out string errorMessage);
    }

    /// <summary>
    /// 유효성 검증을 건너뛰는 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkipFieldValidAttribute : Attribute
    {
    }

    /// <summary>
    /// 특정 필드에 대한 모든 유효성 검증 규칙을 제거하는 어트리뷰트
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RemoveFieldAttribute : Attribute
    {
        /// <summary>
        /// 유효성 검증을 제거할 필드 이름
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// 필드에 대한 유효성 검증을 제거하는 어트리뷰트 생성
        /// </summary>
        /// <param name="fieldName">유효성 검증을 제거할 필드 이름</param>
        public RemoveFieldAttribute(string fieldName)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        }
    }
}