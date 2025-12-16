namespace SLK.NT.Common.Model
{
    /// <summary>
    /// Login request DTO
    /// 
    /// Used for:
    /// - Username/password authentication
    /// - API login (JWT issuance)
    /// 
    /// Notes:
    /// - Keep this DTO minimal but extensible.
    /// - Device / Client info is often used for audit, logging, and security.
    /// </summary>
    public class NTLoginRequest
    {
        /// <summary>
        /// Login identifier.
        /// 
        /// Usually:
        /// - IdentityUser.UserName
        /// - or Email (depending on policy)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password.
        /// 
        /// - Never log this value.
        /// - Transmitted only over HTTPS.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Client application identifier.
        /// 
        /// Examples:
        /// - "admin-web"
        /// - "front-web"
        /// - "mobile-app"
        /// 
        /// Used for:
        /// - Audit logs
        /// - Token audience / scope decisions (advanced)
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client application version.
        /// 
        /// Examples:
        /// - "1.0.3"
        /// - "2025.01"
        /// 
        /// Used for:
        /// - Debugging
        /// - Gradual rollout / compatibility checks
        /// </summary>
        public string? ClientVersion { get; set; }

        /// <summary>
        /// Optional device identifier.
        /// 
        /// Examples:
        /// - Browser fingerprint
        /// - Mobile device id
        /// 
        /// Used for:
        /// - Security audit
        /// - Refresh token binding (future)
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Client IP address (optional).
        /// 
        /// Usually filled by:
        /// - API Gateway
        /// - Reverse Proxy
        /// - Or server-side extraction
        /// 
        /// Kept here for explicit audit scenarios.
        /// </summary>
        public string? ClientIp { get; set; }
    }
}
