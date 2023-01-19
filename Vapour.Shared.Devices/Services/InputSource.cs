using System.Text;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
internal class InputSource : IInputSource
{
    private ICompatibleHidDevice _controller1;
    private byte[] _controller1InputReportBuffer;

    public ICompatibleHidDevice Controller1
    {
        get => _controller1;
        set
        {
            _controller1 = value;
            _controller1InputReportBuffer = new byte[_controller1.SourceDevice.InputReportByteLength];
        }
    }

    public ICompatibleHidDevice Controller2 { get; set; }

    public event EventHandler<InputSourceConfiguration> ConfigurationChanged;
    public InputSourceConfiguration Configuration { get; private set; }

    public string InputSourceKey
    {
        get
        {
            var builder = new StringBuilder();
            if (Controller1 != null)
            {
                builder.Append(Controller1.DeviceKey);
            }

            if (Controller2 != null)
            {
                builder.AppendFormat("::{0}", Controller2.DeviceKey);
            }

            return builder.ToString();
        }
    }

    public void SetConfiguration(InputSourceConfiguration configuration)
    {
        Configuration = configuration;
        Controller1.SetConfiguration(Configuration);
        ConfigurationChanged?.Invoke(this, configuration);
    }

    public InputSourceReport ProcessInputReport(ReadOnlySpan<byte> input)
    {
        Controller1.ProcessInputReport(input);
        return Controller1.InputSourceReport;
    }

    public byte[] ReadInputReport()
    {
        Controller1.ReadInputReport(_controller1InputReportBuffer);
        return _controller1InputReportBuffer;
    }

    public void OnAfterStartListening()
    {
        Controller1.OnAfterStartListening();
    }
}