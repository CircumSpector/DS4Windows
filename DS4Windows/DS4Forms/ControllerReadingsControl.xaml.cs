using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using DS4WinWPF.Translations;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for ControllerReadingsControl.xaml
    /// </summary>
    public partial class ControllerReadingsControl : UserControl
    {
        private const int CANVAS_WIDTH = 130;
        private const int CANVAS_MIDPOINT = CANVAS_WIDTH / 2;
        private const double TRIG_LB_TRANSFORM_OFFSETY = 66.0;
        private readonly ActivitySource activitySource = new(Constants.ApplicationName);
        private readonly DS4State baseState = new();

        private int deviceNum;
        private readonly DS4StateExposed exposeState;
        private readonly DS4State interState = new();
        private double l2Dead;
        private double lsDeadX;
        private double lsDeadY;
        private LatencyWarnMode prevWarnMode;
        private int profileDeviceNum;
        private double r2Dead;
        private readonly NonFormTimer readingTimer;
        private double rsDeadX;
        private double rsDeadY;

        private double sixAxisXDead;
        private double sixAxisZDead;
        private bool useTimer;

        private LatencyWarnMode warnMode;

        public ControllerReadingsControl()
        {
            using var activity = activitySource.StartActivity(
                $"{nameof(ControllerReadingsControl)}:Constructor");

            using (activitySource.StartActivity(
                       $"{nameof(ControllerReadingsControl)}:{nameof(InitializeComponent)}"))
            {
                InitializeComponent();
            }

            inputContNum.Content = $"#{deviceNum + 1}";
            exposeState = new DS4StateExposed(baseState);

            readingTimer = new NonFormTimer();
            readingTimer.Interval = 1000 / 60.0;

            LsDeadXChanged += ChangeLsDeadControls;
            LsDeadYChanged += ChangeLsDeadControls;

            RsDeadXChanged += ChangeRsDeadControls;
            RsDeadYChanged += ChangeRsDeadControls;

            SixAxisDeadXChanged += ChangeSixAxisDeadControls;
            SixAxisDeadZChanged += ChangeSixAxisDeadControls;
            DeviceNumChanged += ControllerReadingsControl_DeviceNumChanged;
        }

        public double LsDeadX
        {
            get => lsDeadX;
            set
            {
                lsDeadX = value;
                LsDeadXChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double LsDeadY
        {
            get => lsDeadY;
            set
            {
                lsDeadY = value;
                LsDeadYChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RsDeadX
        {
            get => rsDeadX;
            set
            {
                rsDeadX = value;
                RsDeadXChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double RsDeadY
        {
            get => rsDeadY;
            set
            {
                rsDeadY = value;
                RsDeadYChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SixAxisXDead
        {
            get => sixAxisXDead;
            set
            {
                sixAxisXDead = value;
                SixAxisDeadXChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double SixAxisZDead
        {
            get => sixAxisZDead;
            set
            {
                sixAxisZDead = value;
                SixAxisDeadZChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double L2Dead
        {
            get => l2Dead;
            set
            {
                l2Dead = value;
                L2DeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double R2Dead
        {
            get => r2Dead;
            set
            {
                r2Dead = value;
                R2DeadChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private event EventHandler DeviceNumChanged;
        public event EventHandler LsDeadXChanged;
        public event EventHandler LsDeadYChanged;
        public event EventHandler RsDeadXChanged;
        public event EventHandler RsDeadYChanged;
        public event EventHandler SixAxisDeadXChanged;
        public event EventHandler SixAxisDeadZChanged;
        public event EventHandler L2DeadChanged;
        public event EventHandler R2DeadChanged;

        private void ControllerReadingsControl_DeviceNumChanged(object sender, EventArgs e)
        {
            inputContNum.Content = $"#{deviceNum + 1}";
        }

        private void ChangeSixAxisDeadControls(object sender, EventArgs e)
        {
            sixAxisDeadEllipse.Width = sixAxisXDead * CANVAS_WIDTH;
            sixAxisDeadEllipse.Height = sixAxisZDead * CANVAS_WIDTH;
            Canvas.SetLeft(sixAxisDeadEllipse, CANVAS_MIDPOINT - sixAxisXDead * CANVAS_WIDTH / 2.0);
            Canvas.SetTop(sixAxisDeadEllipse, CANVAS_MIDPOINT - sixAxisZDead * CANVAS_WIDTH / 2.0);
        }

        private void ChangeRsDeadControls(object sender, EventArgs e)
        {
            rsDeadEllipse.Width = rsDeadX * CANVAS_WIDTH;
            rsDeadEllipse.Height = rsDeadY * CANVAS_WIDTH;
            Canvas.SetLeft(rsDeadEllipse, CANVAS_MIDPOINT - rsDeadX * CANVAS_WIDTH / 2.0);
            Canvas.SetTop(rsDeadEllipse, CANVAS_MIDPOINT - rsDeadY * CANVAS_WIDTH / 2.0);
        }

        private void ChangeLsDeadControls(object sender, EventArgs e)
        {
            lsDeadEllipse.Width = lsDeadX * CANVAS_WIDTH;
            lsDeadEllipse.Height = lsDeadY * CANVAS_WIDTH;
            Canvas.SetLeft(lsDeadEllipse, CANVAS_MIDPOINT - lsDeadX * CANVAS_WIDTH / 2.0);
            Canvas.SetTop(lsDeadEllipse, CANVAS_MIDPOINT - lsDeadY * CANVAS_WIDTH / 2.0);
        }

        public void UseDevice(int index, int profileDevIdx)
        {
            deviceNum = index;
            profileDeviceNum = profileDevIdx;
            DeviceNumChanged?.Invoke(this, EventArgs.Empty);
        }

        public void EnableControl(bool state)
        {
            if (state)
            {
                IsEnabled = true;
                useTimer = true;
                readingTimer.Elapsed += ControllerReadingTimer_Elapsed;
                readingTimer.Start();
            }
            else
            {
                IsEnabled = false;
                useTimer = false;
                readingTimer.Elapsed -= ControllerReadingTimer_Elapsed;
                readingTimer.Stop();
            }
        }

        private void ControllerReadingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            readingTimer.Stop();

            var ds = ControlService.CurrentInstance.DS4Controllers[deviceNum];
            if (ds != null)
            {
                // Don't bother waiting for UI thread to grab references
                //DS4StateExposed tmpexposeState = ControlService.CurrentInstance.ExposedState[deviceNum];
                var tmpbaseState = ControlService.CurrentInstance.GetDs4State(deviceNum);
                var tmpinterState = ControlService.CurrentInstance.GetDs4StateTemp(deviceNum);
                var cntCalibrating = ds.SixAxis.CntCalibrating;

                // Wait for controller to be in a wait period
                ds.ReadWaitEv.Wait();
                ds.ReadWaitEv.Reset();

                // Make copy of current state values for UI thread
                tmpbaseState.CopyTo(baseState);
                tmpinterState.CopyTo(interState);

                if (deviceNum != profileDeviceNum)
                    Mapping.SetCurveAndDeadzone(profileDeviceNum, baseState, interState);

                // Done with copying. Allow input thread to resume
                ds.ReadWaitEv.Set();

                Dispatcher.Invoke(() =>
                {
                    int x = baseState.LX;
                    int y = baseState.LY;

                    Canvas.SetLeft(lsValRec, x / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetTop(lsValRec, y / 255.0 * CANVAS_WIDTH - 3);
                    //bool mappedLS = interState.LX != x || interState.LY != y;
                    //if (mappedLS)
                    //{
                    Canvas.SetLeft(lsMapValRec, interState.LX / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetTop(lsMapValRec, interState.LY / 255.0 * CANVAS_WIDTH - 3);
                    //}

                    x = baseState.RX;
                    y = baseState.RY;
                    Canvas.SetLeft(rsValRec, x / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetTop(rsValRec, y / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetLeft(rsMapValRec, interState.RX / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetTop(rsMapValRec, interState.RY / 255.0 * CANVAS_WIDTH - 3);

                    x = exposeState.getAccelX() + 127;
                    y = exposeState.getAccelZ() + 127;
                    Canvas.SetLeft(sixAxisValRec, x / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetTop(sixAxisValRec, y / 255.0 * CANVAS_WIDTH - 3);
                    Canvas.SetLeft(sixAxisMapValRec,
                        Math.Min(Math.Max(interState.Motion.outputAccelX + 127.0, 0), 255.0) / 255.0 * CANVAS_WIDTH -
                        3);
                    Canvas.SetTop(sixAxisMapValRec,
                        Math.Min(Math.Max(interState.Motion.outputAccelZ + 127.0, 0), 255.0) / 255.0 * CANVAS_WIDTH -
                        3);

                    l2Slider.Value = baseState.L2;
                    l2ValLbTrans.Y = Math.Min(interState.L2, Math.Max(0, 255)) / 255.0 * -70.0 +
                                     TRIG_LB_TRANSFORM_OFFSETY;
                    if (interState.L2 >= 255)
                        l2ValLbBrush.Color = Colors.Green;
                    else if (interState.L2 == 0)
                        l2ValLbBrush.Color = Colors.Red;
                    else
                        l2ValLbBrush.Color = Colors.Black;

                    r2Slider.Value = baseState.R2;
                    r2ValLbTrans.Y = Math.Min(interState.R2, Math.Max(0, 255)) / 255.0 * -70.0 +
                                     TRIG_LB_TRANSFORM_OFFSETY;
                    if (interState.R2 >= 255)
                        r2ValLbBrush.Color = Colors.Green;
                    else if (interState.R2 == 0)
                        r2ValLbBrush.Color = Colors.Red;
                    else
                        r2ValLbBrush.Color = Colors.Black;

                    gyroYawSlider.Value = baseState.Motion.gyroYawFull;
                    gyroPitchSlider.Value = baseState.Motion.gyroPitchFull;
                    gyroRollSlider.Value = baseState.Motion.gyroRollFull;

                    accelXSlider.Value = exposeState.getAccelX();
                    accelYSlider.Value = exposeState.getAccelY();
                    accelZSlider.Value = exposeState.getAccelZ();

                    touchXValLb.Content = baseState.TrackPadTouch0.X;
                    touchYValLb.Content = baseState.TrackPadTouch0.Y;

                    var latency = ds.Latency;
                    var warnInterval = ds.getWarnInterval();
                    inputDelayLb.Content = string.Format(Properties.Resources.InputDelay,
                        latency.ToString());

                    if (latency > warnInterval)
                    {
                        warnMode = LatencyWarnMode.Warn;
                        inpuDelayBackBrush.Color = Colors.Red;
                        inpuDelayForeBrush.Color = Colors.White;
                    }
                    else if (latency > warnInterval * 0.5)
                    {
                        warnMode = LatencyWarnMode.Caution;
                        inpuDelayBackBrush.Color = Colors.Yellow;
                        inpuDelayForeBrush.Color = Colors.Black;
                    }
                    else
                    {
                        warnMode = LatencyWarnMode.None;
                        inpuDelayBackBrush.Color = Colors.Transparent;
                        inpuDelayForeBrush.Color = SystemColors.WindowTextColor;
                    }

                    prevWarnMode = warnMode;

                    batteryLvlLb.Content = $"{Strings.Battery}: {baseState.Battery}%";
                    gyroCalEllipse.Visibility = cntCalibrating > 0 && cntCalibrating / 250 % 2 == 1
                        ? Visibility.Visible
                        : Visibility.Hidden;
                    UpdateCoordLabels(baseState, interState, exposeState);
                });
            }

            if (useTimer) readingTimer.Start();
        }

        private void UpdateCoordLabels(DS4State inState, DS4State mapState,
            DS4StateExposed exposeState)
        {
            lxInValLb.Content = inState.LX;
            lxOutValLb.Content = mapState.LX;
            lyInValLb.Content = inState.LY;
            lyOutValLb.Content = mapState.LY;

            rxInValLb.Content = inState.RX;
            rxOutValLb.Content = mapState.RX;
            ryInValLb.Content = inState.RY;
            ryOutValLb.Content = mapState.RY;

            sixAxisXInValLb.Content = exposeState.AccelX;
            sixAxisXOutValLb.Content = mapState.Motion.outputAccelX;
            sixAxisZInValLb.Content = exposeState.AccelZ;
            sixAxisZOutValLb.Content = mapState.Motion.outputAccelZ;

            l2InValLb.Content = inState.L2;
            l2OutValLb.Content = mapState.L2;
            r2InValLb.Content = inState.R2;
            r2OutValLb.Content = mapState.R2;
        }

        private enum LatencyWarnMode : uint
        {
            None,
            Caution,
            Warn
        }
    }
}