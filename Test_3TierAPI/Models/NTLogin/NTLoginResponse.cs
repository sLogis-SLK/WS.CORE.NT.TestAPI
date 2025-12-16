namespace SLK.NT.Common.Model
{
    /// <summary>
    /// Login response DTO
    /// 
    /// Returned after successful authentication.
    /// Contains issued access token and metadata.
    /// </summary>
    public class NTLoginResponse
    {
        /// <summary>
        /// JWT access token.
        /// 
        /// Usage:
        /// Authorization: Bearer {AccessToken}
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time (UTC).
        /// 
        /// Used by:
        /// - Frontend token refresh scheduling
        /// - Client-side session handling
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Authenticated user identifier.
        /// 
        /// Matches:
        /// - IdentityUser.Id
        /// - JWT ClaimTypes.NameIdentifier
        /// - SignalR UserIdentifier
        /// 
        /// Frontend can cache this safely.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User display name.
        /// 
        /// Usually:
        /// - UserName
        /// - Or NickName / DisplayName (if extended later)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Assigned roles.
        /// 
        /// Useful for:
        /// - UI rendering (menu, admin pages)
        /// - Quick client-side checks
        /// 
        /// Server-side authorization must still rely on JWT claims.
        /// </summary>
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Granted permissions.
        /// 
        /// Examples:
        /// - "Order.Create"
        /// - "Order.Cancel"
        /// 
        /// Optional but frequently used in enterprise frontends.
        /// </summary>
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
