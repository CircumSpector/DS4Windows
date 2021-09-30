using Serilog;
using Serilog.Configuration;

namespace DS4WinWPF.DS4Control.Logging
{
    public static class SerilogInMemoryLoggerConfigurationExtensions
    {
        public static LoggerConfiguration SerilogInMemorySink(
            this LoggerSinkConfiguration sinkConfiguration)
        {
            return sinkConfiguration.Sink(new InMemorySink());
        }
    }
}