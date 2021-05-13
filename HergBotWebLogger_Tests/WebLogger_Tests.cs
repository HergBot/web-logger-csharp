using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HergBot.Logging;
using HergBot.Logging.WebLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace HergBotWebLogger_Tests
{
    public class MockLogger : ILogger
    {
        public List<string> Messages { get; private set; }

        public MockLogger()
        {
            Messages = new List<string>();
        }

        public void LogDebug(string debugMessage, string methodName = "")
        {
            Messages.Add(debugMessage);
        }

        public void LogError(string errorMessage, string methodName = "")
        {
            Messages.Add(errorMessage);
        }

        public void LogException(Exception exception, string methodName = "")
        {
            Messages.Add(exception.Message);
        }

        public void LogInfo(string infoMessage, string methodName = "")
        {
            Messages.Add(infoMessage);
        }

        public void LogWarning(string warningMessage, string methodName = "")
        {
            Messages.Add(warningMessage);
        }
    }

    public class WebLogger_Tests
    {
        private const string AUTH_HEADER = "Authorization";

        private const string AUTH_TOKEN = "test-token";

        private const string TEST_PATH = "/test-path";

        private const string LONG_PATH = "/Rp2rjBNRwBgaZXK8kLqcdyW2MaEeKG7CaciVSf7MESwfWLAft83t6KdN6g6TBJ8XrnWUrSZzqNMLS3mKeZEb6wjH5RXwfqwDNqPfZbNC4ZiPNqF63uRjBWgRezwD8dpqeZHwEpJmbuKmqvCnAbLzNcDbjTRSmPGUqrebDGWAAbZuNpd2ht3VkHnqDHYPhPRkSviuK7AHqUvvA32UtBQEFe7AYgXvSQQyCmcG7pxcQyD2XiUUMSiYT5R73Azp7xeDQWP4Se9kJzUVuYpiiXnirQGXc2La883FGzuNZzNvYtUYtWN5UpRcwrwa34tKJ2eiGQP9zS96vqHkJ42t8g37qya7diTb5FLqynABAZED7heTfpQw8m87bZiEUTaHBq9PAjKKgAcPepEp2aA3ijWprHigUuJZvLkP8aEtvSarjZdtJ5LhJikmdnpJMbaQjhra85eRc2SW3GhR6edti9WSbzuCgpAErB378r8NQNQ2PC7M5DJzkykdXJBxMCxBTc5aB5JEzR4c5A5kavhXnumLqhwjfb7PcSpMazfp7xVqrN5VLK88kVG7qEY9axVr5D3NHEnE2N2CqqxSWux552TNdnuH";

        private const string LONG_STRING = "]@/CAyEh}Efgx*6yYM_cp9Av99z_%]Vyia9wq)k#NJ2}QHmz;2=Xvn@Jgec{fmu)QN{PZiMp5!QX8N55pV(%D[TC_zftPp?k}r6R_@H{[y_UtH;+_mX#Lg@v";

        private const string LONG_QUERY = "?keyOne=NNuKmUJuVzyEYvaYBMYgkhnUdUppqpVtmLBXSwrV2yni97pLfW3285bCMGiHAvXdpMf6NZwWHjLCJANqV7LifNW6nAH2S88jJ9Yb6NYy3GrRcYqhumtijUmu";

        private MockLogger _mockLog;

        private WebLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _mockLog = new MockLogger();
            _logger = new WebLogger(_mockLog);
        }

        [Test]
        public void LogRequest_ReturnsUuid()
        {
            HttpRequest request = CreateRequest(
                "GET",
                TEST_PATH,
                new string[] { },
                string.Empty,
                string.Empty
            );
            string requestId = _logger.LogRequest(request);
            Assert.NotNull(requestId);
        }

        [TestCase("GET", TEST_PATH, null, null, null)]
        [TestCase("GET", TEST_PATH, new string[] { AUTH_TOKEN }, null, null)]
        [TestCase("GET", TEST_PATH, new string[] { AUTH_TOKEN }, "?keyOne=valueOne", null)]
        [TestCase("GET", TEST_PATH, new string[] { AUTH_TOKEN }, "?keyOne=valueOne", "body content")]
        public void LogRequest_RequestInfo(string method, string path, string[] tokens, string queryString, string body)
        {
            HttpRequest request = CreateRequest(
                method,
                path,
                tokens ?? new string[] {},
                queryString ?? string.Empty,
                body ?? string.Empty
            );
            string requestId = _logger.LogRequest(request);
            Assert.IsTrue(_mockLog.Messages[0].Contains($"Request {requestId}"));
            Assert.IsTrue(_mockLog.Messages[1].Contains($"{method} {path}"));
            Assert.IsTrue(tokens == null ? _mockLog.Messages[2].Contains("N/A") : tokens.All(token => _mockLog.Messages[2].Contains(token)));
            Assert.IsTrue(queryString == null ? _mockLog.Messages[3].Contains("N/A") : _mockLog.Messages[3].Contains(queryString));
            Assert.IsTrue(body == null ? _mockLog.Messages[4].Contains("N/A") : _mockLog.Messages[4].Contains(body));
        }

        [TestCase("GET", LONG_PATH, null, null, null)]
        [TestCase("GET", LONG_PATH, new string[] { LONG_STRING }, null, null)]
        [TestCase("GET", LONG_PATH, new string[] { LONG_STRING }, LONG_QUERY, null)]
        [TestCase("GET", LONG_PATH, new string[] { LONG_STRING }, LONG_QUERY, LONG_STRING)]
        public void LogRequest_TruncatedInfo(string method, string path, string[] tokens, string queryString, string body)
        {
            HttpRequest request = CreateRequest(
                method,
                path,
                tokens ?? new string[] { },
                queryString ?? string.Empty,
                body ?? string.Empty
            );
            string requestId = _logger.LogRequest(request);
            Assert.IsTrue(_mockLog.Messages[0].Contains($"Request {requestId}"));
            Assert.IsTrue(_mockLog.Messages[1].Contains($"{method} {path.Substring(0, 500)}..."));
            Assert.IsTrue(tokens == null ? _mockLog.Messages[2].Contains("N/A") : tokens.All(token => _mockLog.Messages[2].Contains($"{token.Substring(0, 100)}...")));
            Assert.IsTrue(queryString == null ? _mockLog.Messages[3].Contains("N/A") : _mockLog.Messages[3].Contains($"{queryString.Substring(0, 100)}..."));
            Assert.IsTrue(body == null ? _mockLog.Messages[4].Contains("N/A") : _mockLog.Messages[4].Contains($"{body.Substring(0, 100)}..."));
        }

        private HttpRequest CreateRequest(string method, string path, string[] tokens, string queryString, string body)
        {
            StringValues authTokens = tokens.Length == 1 ? new StringValues(tokens[0]) : new StringValues(tokens);
            HttpContext context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = new PathString(path);
            context.Request.Headers.Add(AUTH_HEADER, authTokens);
            context.Request.QueryString = new QueryString(queryString);
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return context.Request;
        }

        public static string RandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
    }
}
