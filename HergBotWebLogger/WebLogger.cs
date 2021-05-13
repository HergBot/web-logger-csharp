//
// FILE     : WebLogger.cs
// PROJECT  : HergBot Web Logger
// AUTHOR   : xHergz
// DATE     : 2021-05-13
// 

using System;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HergBot.Logging.WebLogging
{
    public class WebLogger
    {
        private ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public WebLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs information about a web request and generates a UUID to attach to identify it
        /// </summary>
        /// <param name="request">The HTTP request information</param>
        /// <returns>A unique ID for the request</returns>
        public string LogRequest(HttpRequest request)
        {
            // Generate request uuid
            string requestId = Guid.NewGuid().ToString();

            // Gather Info
            request.Headers.TryGetValue("Authorization", out StringValues authenticationHeader);
            string authentication = authenticationHeader.Count == 0 ? "N/A" : authenticationHeader.ToString();
            string queryString = request.QueryString.HasValue ? request.QueryString.Value : "N/A";
            string body = request.Body.Length == 0 ? "N/A" : new StreamReader(request.Body).ReadToEnd();

            // Log format
            _logger.LogInfo($"Request {requestId}");
            _logger.LogInfo($"-> {request.Method} {Truncate(request.Path.Value, 500)}");
            _logger.LogInfo($"-> Auth: {Truncate(authentication)}");
            _logger.LogInfo($"-> Query: {Truncate(queryString)}");
            _logger.LogInfo($"-> Body: {Truncate(body)}");

            // Return uuid to attach to context
            return requestId;
        }

        /// <summary>
        /// Truncates a string if it is above the given max. Adds an ellipse if truncated.
        /// </summary>
        /// <param name="str">The string to truncate</param>
        /// <param name="max">The maximum length of the string before truncation (not including added ellipsis)</param>
        /// <returns>The string if <= the max, the truncated string with an ellipsis if > the max</returns>
        private string Truncate(string str, int max = 100)
        {
            return str.Length <= max ? str : $"{str.Substring(0, max)}...";
        }
    }
}
