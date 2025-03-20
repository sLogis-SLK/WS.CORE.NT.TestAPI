namespace Test_3TierAPI.CustomAnnotation
{
    /// <summary>
    /// 각 엔드포인트를 위한 커스텀 Attribute(어노테이션)
    /// [RequireData(true)] 어트리뷰트를 사용하면 해당 엔드포인트에 데이터가 필요한 Endpoint라고 판단하여
    /// RequestValidationMiddleware에서 데이터가 있는지 확인하고 없다면 400 Bad Request를 반환한다.
    /// 어노테이션이 없거나, [RequireData(false)]로 설정하면 데이터가 필요하지 않은 Endpoint로 판단하여 예외처리 검사에서 제외한다.
    /// 클래스와 컨트롤러 함수 모두 사용가능하다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireDataAttribute : Attribute
    {
        public bool IsRequired { get; }

        public RequireDataAttribute(bool isRequired = true)
        {
            IsRequired = isRequired;
        }
    }
}
