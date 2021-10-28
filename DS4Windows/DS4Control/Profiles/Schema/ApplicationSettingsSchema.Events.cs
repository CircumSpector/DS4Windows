using System;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing.Util;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    public partial class DS4WindowsAppSettingsV3
    {
        public event Action<bool> IsTracingEnabledChanged;

        [UsedImplicitly]
        private void OnIsTracingEnabledChanged(object oldValue, object newValue)
        {
            //
            // Automatically register tracer, if configuration value instructs to
            // 
            if ((bool)newValue && !GlobalTracer.IsRegistered())
            {
                // This is necessary to pick the correct sender, otherwise a NoopSender is used!
                Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(new NullLoggerFactory())
                    .RegisterSenderFactory<ThriftSenderFactory>();

                // This will log to a default localhost installation of Jaeger.
                var tracer = new Tracer.Builder(DS4Windows.Constants.ApplicationName)
                    .WithLoggerFactory(new NullLoggerFactory())
                    .WithSampler(new ConstSampler(true))
                    .Build();

                // Allows code that can't use DI to also access the tracer.
                GlobalTracer.Register(tracer);
            }

            IsTracingEnabledChanged?.Invoke((bool)newValue);
        }
    }
}
