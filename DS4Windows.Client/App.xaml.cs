using DS4Windows.Client.Core;
using DS4Windows.Client.Modules.Main;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace DS4Windows.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {   public App()
        {
            ApplicationStartup.Start<MainWindowViewModel, MainWindow>();
        }        
    }
}
