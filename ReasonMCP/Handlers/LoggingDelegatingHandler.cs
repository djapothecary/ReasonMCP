using Microsoft.Extensions.Logging;

namespace ReasonMCP.Handlers
{
    /// <summary>
    /// Delegating handler that logs outgoing HTTP requests and responses for debugging
    /// </summary>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="LoggingDelegatingHandler"/>.
        /// Intentionally using "boilerplate" constructor rather than primary constructor pattern
        /// this preserves immutability of logger
        /// </summary>
        /// <param name="logger">Logger instance used to emit request/response events.</param>
        public LoggingDelegatingHandler(
            ILogger<LoggingDelegatingHandler> logger
        )
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs the outgoing <paramref name="request"/> and resulting response.
        /// </summary>
        /// <param name="request">The HTTP request message being sent.</param>
        /// <param name="cancellationToken">CancellationToken for the send operation.</param>
        /// <returns>The received <see cref="HttpResponseMessage"/>.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            _logger.LogInformation("Sending HTTP request to Method: {request.Method} URL: {request.RequestUri} .", request.Method, request.RequestUri);

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("Received HTTP response with Status Code: {response.StatusCode} for Uri: {request.RequestUri} .", request.Method, request.RequestUri);

            return response;
        }

        private async Task<string> ReadAndTruncateContentAsync(HttpContent? content)
        {
            if (content == null)
                return "No Content.";

            var rawString = await content.ReadAsStringAsync();

            //  Chop massive payloads
            if (rawString.Length > 2000)
            {
                return string.Concat(rawString.AsSpan(0, 2000), " .... [TRUNCATED FOR LOGGING]");
            }

            return rawString;
        }
    }
}