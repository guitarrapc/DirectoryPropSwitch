using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace DirectoryPropSwitch.Tests
{
    public class TestOutputLogger : ILogger
    {
        readonly ITestOutputHelper output;
        readonly LogLevel minimumLogLevel;

        public TestOutputLogger(ITestOutputHelper output, LogLevel minimumLogLevel)
        {
            this.output = output;
            this.minimumLogLevel = minimumLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return minimumLogLevel <= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));
            if (minimumLogLevel > logLevel) return;

            var msg = formatter(state, exception);
            if (!string.IsNullOrEmpty(msg))
            {
                output.WriteLine(msg);
            }

            if (exception != null)
            {
                output.WriteLine(exception.ToString());
            }
        }

        public class NullDisposable : IDisposable
        {
            public static readonly IDisposable Instance = new NullDisposable();

            public void Dispose()
            {
            }
        }
    }
}
