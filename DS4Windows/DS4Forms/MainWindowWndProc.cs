using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    public partial class MainWindow
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = PresentationSource.FromVisual(this) as HwndSource;
            HookWindowMessages(source);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                case Util.WM_DEVICECHANGE:
                {
                    if (Global.Instance.RunHotPlug)
                    {
                        var Type = wParam.ToInt32();
                        if (Type == DBT_DEVICEARRIVAL ||
                            Type == DBT_DEVICEREMOVECOMPLETE)
                        {
                            lock (hotplugCounterLock)
                            {
                                hotplugCounter++;
                            }

                            if (!inHotPlug)
                            {
                                inHotPlug = true;
                                var hotplugTask = Task.Run(HandleDeviceArrivalRemoval);
                                // Log exceptions that might occur
                                Util.LogAssistBackgroundTask(hotplugTask);
                            }
                        }
                    }

                    break;
                }
                case WM_COPYDATA:
                {
                    // Received InterProcessCommunication (IPC) message. DS4Win command is embedded as a string value in lpData buffer
                    try
                    {
                        var cds = (App.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(App.COPYDATASTRUCT));
                        if (cds.cbData >= 4 && cds.cbData <= 256)
                        {
                            var tdevice = -1;

                            var buffer = new byte[cds.cbData];
                            Marshal.Copy(cds.lpData, buffer, 0, cds.cbData);
                            var strData = Encoding.ASCII.GetString(buffer).Split('.');

                            if (strData.Length >= 1)
                            {
                                strData[0] = strData[0].ToLower();

                                if (strData[0] == "start")
                                {
                                    if (!ControlService.CurrentInstance.IsRunning)
                                        ChangeService();
                                }
                                else if (strData[0] == "stop")
                                {
                                    if (ControlService.CurrentInstance.IsRunning)
                                        ChangeService();
                                }
                                else if (strData[0] == "cycle")
                                {
                                    ChangeService();
                                }
                                else if (strData[0] == "shutdown")
                                {
                                    // Force disconnect all gamepads before closing the app to avoid "Are you sure you want to close the app" messagebox
                                    if (ControlService.CurrentInstance.IsRunning)
                                        ChangeService();

                                    // Call closing method and let it to close editor wnd (if it is open) before proceeding to the actual "app closed" handler
                                    MainDS4Window_Closing(null, new CancelEventArgs());
                                    MainDS4Window_Closed(this, new EventArgs());
                                }
                                else if (strData[0] == "disconnect")
                                {
                                    // Command syntax: Disconnect[.device#] (fex Disconnect.1)
                                    // Disconnect all wireless controllers. ex. (Disconnect)
                                    if (strData.Length == 1)
                                    {
                                        // Attempt to disconnect all wireless controllers
                                        // Opt to make copy of Dictionary before iterating over contents
                                        var dictCopy =
                                            new Dictionary<int, CompositeDeviceModel>(conLvViewModel.ControllerDict);
                                        foreach (var pair in dictCopy) pair.Value.RequestDisconnect();
                                    }
                                    else
                                    {
                                        // Attempt to disconnect one wireless controller
                                        if (int.TryParse(strData[1], out tdevice)) tdevice--;

                                        if (conLvViewModel.ControllerDict.TryGetValue(tdevice, out var model))
                                            model.RequestDisconnect();
                                    }
                                }
                                else if (strData[0] == "changeledcolor" && strData.Length >= 5)
                                {
                                    // Command syntax: changeledcolor.device#.red.gree.blue (ex changeledcolor.1.255.0.0)
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;
                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        byte.TryParse(strData[2], out var red);
                                        byte.TryParse(strData[3], out var green);
                                        byte.TryParse(strData[4], out var blue);

                                        conLvViewModel.ControllerCol[tdevice]
                                            .UpdateCustomLightColor(Color.FromRgb(red, green, blue));
                                    }
                                }
                                else if ((strData[0] == "loadprofile" || strData[0] == "loadtempprofile") &&
                                         strData.Length >= 3)
                                {
                                    // Command syntax: LoadProfile.device#.profileName (fex LoadProfile.1.GameSnake or LoadTempProfile.1.WebBrowserSet)
                                    if (int.TryParse(strData[1], out tdevice)) tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT &&
                                        File.Exists(Path.Combine(Global.RuntimeAppDataPath,
                                            Constants.ProfilesSubDirectory, strData[2] + ".xml")))
                                    {
                                        if (strData[0] == "loadprofile")
                                        {
                                            var idx = ProfileListHolder.ProfileListCollection
                                                .Select((item, index) => new { item, index })
                                                .Where(x => x.item.Name == strData[2]).Select(x => x.index)
                                                .DefaultIfEmpty(-1).First();

                                            if (idx >= 0 && tdevice < conLvViewModel.ControllerCol.Count)
                                                conLvViewModel.ControllerCol[tdevice].ChangeSelectedProfile(strData[2]);
                                            else
                                                // Preset profile name for later loading
                                                Global.Instance.Config.ProfilePath[tdevice] = strData[2];
                                            //Global.LoadProfile(tdevice, true, ControlService.CurrentInstance);
                                        }
                                        else
                                        {
                                            Global.Instance.LoadTempProfile(tdevice, strData[2], true,
                                                ControlService.CurrentInstance).Wait();
                                        }

                                        var device = conLvViewModel.ControllerCol[tdevice].Device;
                                        if (device != null)
                                        {
                                            var prolog = string.Format(Properties.Resources.UsingProfile,
                                                (tdevice + 1).ToString(), strData[2], $"{device.Battery}");
                                            ControlService.CurrentInstance.LogDebug(prolog);
                                        }
                                    }
                                }
                                else if (strData[0] == "outputslot" && strData.Length >= 3)
                                {
                                    // Command syntax: 
                                    //    OutputSlot.slot#.Unplug
                                    //    OutputSlot.slot#.PlugDS4
                                    //    OutputSlot.slot#.PlugX360
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        strData[2] = strData[2].ToLower();
                                        var slotDevice =
                                            ControlService.CurrentInstance.OutputslotMan.OutputSlots[tdevice];
                                        if (strData[2] == "unplug")
                                            ControlService.CurrentInstance.DetachUnboundOutDev(slotDevice);
                                        else if (strData[2] == "plugds4")
                                            ControlService.CurrentInstance.AttachUnboundOutDev(slotDevice,
                                                OutContType.DS4);
                                        else if (strData[2] == "plugx360")
                                            ControlService.CurrentInstance.AttachUnboundOutDev(slotDevice,
                                                OutContType.X360);
                                    }
                                }
                                else if (strData[0] == "query" && strData.Length >= 3)
                                {
                                    string propName;
                                    var propValue = string.Empty;

                                    // Command syntax: QueryProfile.device#.Name (fex "Query.1.ProfileName" would print out the name of the active profile in controller 1)
                                    if (int.TryParse(strData[1], out tdevice))
                                        tdevice--;

                                    if (tdevice >= 0 && tdevice < ControlService.MAX_DS4_CONTROLLER_COUNT)
                                    {
                                        // Name of the property to query from a profile or DS4Windows app engine
                                        propName = strData[2].ToLower();

                                        if (propName == "profilename")
                                        {
                                            if (Global.UseTempProfiles[tdevice])
                                                propValue = Global.TempProfileNames[tdevice];
                                            else
                                                propValue = Global.Instance.Config.ProfilePath[tdevice];
                                        }
                                        /*
                                        else if (propName == "outconttype")
                                        {
                                            propValue = Global.Instance.Config.OutputDeviceType[tdevice].ToString();
                                        }
                                        */
                                        else if (propName == "activeoutdevtype")
                                        {
                                            propValue = Global.ActiveOutDevType[tdevice].ToString();
                                        }
                                        /*
                                        else if (propName == "usedinputonly")
                                        {
                                            propValue = Global.DIOnly[tdevice].ToString();
                                        }
                                        */

                                        else if (propName == "devicevidpid" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue =
                                                $"VID={rootHub.DS4Controllers[tdevice].HidDevice.Attributes.VendorHexId}, PID={rootHub.DS4Controllers[tdevice].HidDevice.Attributes.ProductHexId}";
                                        }
                                        else if (propName == "devicepath" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].HidDevice.DevicePath;
                                        }
                                        else if (propName == "macaddress" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].MacAddress.ToFriendlyName();
                                        }
                                        else if (propName == "displayname" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].DisplayName;
                                        }
                                        else if (propName == "conntype" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].ConnectionType.ToString();
                                        }
                                        else if (propName == "exclusivestatus" &&
                                                 rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].CurrentExclusiveStatus
                                                .ToString();
                                        }
                                        else if (propName == "battery" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].Battery.ToString();
                                        }
                                        else if (propName == "charging" && rootHub.DS4Controllers[tdevice] != null)
                                        {
                                            propValue = rootHub.DS4Controllers[tdevice].Charging.ToString();
                                        }
                                        else if (propName == "outputslottype")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].CurrentType
                                                .ToString();
                                        }
                                        else if (propName == "outputslotpermanenttype")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].PermanentType
                                                .ToString();
                                        }
                                        else if (propName == "outputslotattachedstatus")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice]
                                                .CurrentAttachedStatus.ToString();
                                        }
                                        else if (propName == "outputslotinputbound")
                                        {
                                            propValue = rootHub.OutputslotMan.OutputSlots[tdevice].CurrentInputBound
                                                .ToString();
                                        }

                                        else if (propName == "apprunning")
                                        {
                                            propValue = rootHub.IsRunning
                                                .ToString(); // Controller idx value is ignored, but it still needs to be in 1..4 range in a cmdline call
                                        }
                                    }

                                    // Write out the property value to MMF result data file and notify a client process that the data is available
                                    (Application.Current as App).WriteIPCResultDataMMF(propValue);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Eat all exceptions in WM_COPYDATA because exceptions here are not fatal for DS4Windows background app
                    }

                    break;
                }
            }

            return IntPtr.Zero;
        }

        private async void HandleDeviceArrivalRemoval()
        {
            inHotPlug = true;

            var loopHotplug = false;
            lock (hotplugCounterLock)
            {
                loopHotplug = hotplugCounter > 0;
            }

            ControlService.CurrentInstance.UpdateHidHiddenAttributes();

            //
            // TODO: WTF?!
            // 
            while (loopHotplug)
            {
                //
                // TODO: WTF?!
                // 
                Thread.Sleep(HOTPLUG_CHECK_DELAY);

                await ControlService.CurrentInstance.HotPlug();

                lock (hotplugCounterLock)
                {
                    hotplugCounter--;
                    loopHotplug = hotplugCounter > 0;
                }
            }

            inHotPlug = false;
        }

        private void HookWindowMessages(HwndSource source)
        {
            var hidGuid = new Guid();

            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            var result = Util.RegisterNotify(source.Handle, hidGuid, ref regHandle);

            if (!result) Application.Current.Shutdown();
        }
    }
}
