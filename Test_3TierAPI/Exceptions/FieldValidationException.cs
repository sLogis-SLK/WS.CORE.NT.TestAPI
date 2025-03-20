using System;
using System.Collections.Generic;

namespace Test_3TierAPI.Exceptions
{
    /// <summary>
    /// 필드 유효성 검증 실패 시 발생하는 예외
    /// </summary>
    public class FieldValidationException : Exception
    {
        /// <summary>
        /// 검증 오류 목록
        /// </summary>
        public List<string> ValidationErrors { get; }

        /// <summary>
        /// 단일 오류 메시지와 함께 예외 생성
        /// </summary>
        public FieldValidationException(string message) : base(message)
        {
            ValidationErrors = new List<string> { message };
        }

        /// <summary>
        /// 여러 오류 메시지와 함께 예외 생성
        /// </summary>
        public FieldValidationException(string message, List<string> errors) : base(message)
        {
            ValidationErrors = errors ?? new List<string>();
        }
    }
}