namespace Test_3TierAPI.CustomAnnotation
{
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
