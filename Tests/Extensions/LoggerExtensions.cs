using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests
{
    public static class LoggerExtensions
    {
        // Thanks to https://adamstorr.azurewebsites.net/blog/mocking-ilogger-with-moq
        public static void VerifyLoggedInformation<T>(this Mock<ILogger<T>> logger, string expectedMessage)
        {
            Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(logger => logger == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}