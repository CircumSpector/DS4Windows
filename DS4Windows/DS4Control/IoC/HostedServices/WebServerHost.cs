using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    /// <summary>
    ///     Runs embedded web server to host Bezier Curve Editor.
    /// </summary>
    class WebServerHost : BackgroundService
    {
        private readonly WebServer webServer;

        private readonly ILogger<WebServerHost> logger;

        public WebServerHost(WebServer webServer, ILogger<WebServerHost> logger)
        {
            this.webServer = webServer;
            this.logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting web server");

            _ = Task.Run(async () => await webServer.RunAsync(stoppingToken), stoppingToken);

            return Task.CompletedTask;
        }
    }
}
