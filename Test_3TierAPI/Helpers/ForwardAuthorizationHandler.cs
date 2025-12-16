public class ForwardAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ForwardAuthorizationHandler> _logger;

    public ForwardAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ForwardAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"]
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            _logger.LogInformation("Forwarding Authorization header: {Header}",
                authHeader[..20] + "...");

            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", authHeader);
        }
        else
        {
            _logger.LogWarning("No Authorization header found to forward");
        }

        return base.SendAsync(request, cancellationToken);
    }
}