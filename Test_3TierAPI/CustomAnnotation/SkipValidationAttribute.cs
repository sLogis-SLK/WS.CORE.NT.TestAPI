using System;

namespace Test_3TierAPI.CustomAnnotation
{
    /// <summary>
    /// 유효성 검증을 완전히 건너뛰게 하는 어트리뷰트
    /// 컨트롤러 클래스나 액션 메서드에 적용하여 모든 유효성 검증을 비활성화
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkipValidationAttribute : Attribute
    {
        // 추가 속성이나 메서드가 필요 없이 마커 어트리뷰트로 사용
    }
}