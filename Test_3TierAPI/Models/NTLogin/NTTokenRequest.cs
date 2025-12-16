namespace SLK.NT.Common.Model
{
    /// <summary>
    /// OAuth 2.0 Token Request (RFC 6749)
    /// 
    /// Standard grant_type: "password"
    /// </summary>
    public class NTTokenRequest
    {
        /// <summary>
        /// REQUIRED. OAuth 2.0 grant type.
        /// For username/password: "password"
        /// </summary>
        public string GrantType { get; set; } = "password";

        /// <summary>
        /// REQUIRED. Resource owner username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// REQUIRED. Resource owner password.
        /// 
        /// Security:
        /// - Never log this value
        /// - Transmit only over HTTPS
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// OPTIONAL. Scope (space-separated).
        /// 
        /// Examples: "openid profile email"
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Client application identifier (custom extension).
        /// Used for audit logs and client tracking.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Client application version (custom extension).
        /// </summary>
        public string? ClientVersion { get; set; }

        /// <summary>
        /// Device identifier (custom extension).
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// Client IP address (custom extension).
        /// Usually populated server-side.
        /// </summary>
        public string? ClientIp { get; set; }
    }
}
