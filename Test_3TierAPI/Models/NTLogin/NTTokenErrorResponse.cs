namespace SLK.NT.Common.Model
{
    /// <summary>
    /// OAuth 2.0 Error Response (RFC 6749)
    /// 
    /// Standard error format for token endpoint failures.
    /// Automatically returned by ASP.NET Core when validation fails.
    /// </summary>
    public class NTTokenErrorResponse
    {
        /// <summary>
        /// REQUIRED. Error code.
        /// 
        /// Standard values:
        /// - "invalid_request"
        /// - "invalid_client"
        /// - "invalid_grant"
        /// - "unauthorized_client"
        /// - "unsupported_grant_type"
        /// - "invalid_scope"
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// OPTIONAL. Human-readable explanation.
        /// 
        /// Security: Do not expose sensitive details.
        /// Always use generic messages in production.
        /// </summary>
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// OPTIONAL. URI for additional information.
        /// </summary>
        public string? ErrorUri { get; set; }
    }
}