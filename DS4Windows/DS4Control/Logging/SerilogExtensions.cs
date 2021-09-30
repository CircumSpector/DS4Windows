using Serilog;
using Serilog.Configuration;

namespace DS4WinWPF.DS4Control.Logging
{
    public static class SerilogCounterLoggerConfigurationExtensions
    {
        public static LoggerConfiguration SerilogInMemorySink(
            this LoggerSinkConfiguration sinkConfiguration)
        {
            return sinkConfiguration.Sink(new InMemorySink());
        }
    }
}