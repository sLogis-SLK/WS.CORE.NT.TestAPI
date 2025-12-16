namespace Test_3TierAPI.Models.NTLogin
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
    }
}
