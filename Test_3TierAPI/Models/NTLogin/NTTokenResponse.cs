namespace SLK.NT.Common.Model
{
    /// <summary>
    /// OAuth 2.0 Token Response (RFC 6749)
    /// 
    /// Standard response format with required/optional fields per spec.
    /// </summary>
    public class NTTokenResponse
    {
        /// <summary>
        /// REQUIRED. The access token issued by the authorization server.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// REQUIRED. Token type (usually "Bearer").
        /// Indicates how the client uses the access token in requests.
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// RECOMMENDED. Lifetime in seconds of the access token.
        /// 
        /// If omitted, implementation-specific expiration is assumed.
        /// For frontend: ExpiresAt = now + ExpiresIn
        /// </summary>
        public long? ExpiresIn { get; set; }

        /// <summary>
        /// OPTIONAL. Refresh token (if issued).
        /// Used to obtain new access tokens without user credentials.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// OPTIONAL. Scope (space-separated).
        /// Included if different from requested scope.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// OPTIONAL. ID Token (for OpenID Connect).
        /// Contains user identity information as JWT.
        /// </summary>
        public string? IdToken { get; set; }

        // ================================================================
        // Custom Extensions (not in RFC 6749 but common in enterprise)
        // ================================================================

        /// <summary>
        /// Authenticated user identifier.
        /// Matches IdentityUser.Id and JWT sub claim.
        /// Safe to cache on frontend.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// User display name (e.g., UserName, Email, or NickName).
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Assigned roles.
        /// Useful for UI rendering and client-side quick checks.
        /// Server-side authorization must use JWT claims.
        /// </summary>
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Granted permissions (e.g., "Order.Create", "Order.Cancel").
        /// Often used in enterprise frontends for feature toggles.
        /// </summary>
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
