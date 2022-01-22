using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AdonisUI.Controls;
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Converters;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for BindingWindow.xaml
    /// </summary>
    public partial class BindingWindow : AdonisWindow
    {
        public enum ExposeMode : uint
        {
            Full,
            Keyboard
        }

        private readonly Dictionary<Button, BindAssociation> associatedBindings = new();

        private readonly BindingWindowViewModel bindingVM;

        private readonly Dictionary<X360ControlItem, Button> conBtnMap = new();

        private readonly ExposeMode expose;
        private readonly Dictionary<int, Button> keyBtnMap = new();

        private readonly Dictionary<X360ControlItem, Button> mouseBtnMap = new();

        private readonly ControlService rootHub;
        private Button highlightBtn;

        [MissingLocalization]
        public BindingWindow(
            ControlService service,
            int deviceNum,
            DS4ControlSettingsV3 settings,
            ExposeMode expose = ExposeMode.Full
        )
        {
            rootHub = service;

            InitializeComponent();

            this.expose = expose;
            bindingVM = new BindingWindowViewModel(deviceNum, settings);

            Title = settings.Control != DS4ControlItem.None
                ? $"Select action for {EnumDescriptionConverter.GetEnumDescription(settings.Control)}"
                : "Select action";

            guideBtn.Content = "";
            highlightImg.Visibility = Visibility.Hidden;
            highlightLb.Visibility = Visibility.Hidden;

            if (expose == ExposeMode.Full) InitButtonBindings();

            InitKeyBindings();
            InitInfoMaps();

            if (!bindingVM.Using360Mode) InitDS4Canvas();

            bindingVM.ActionBinding = bindingVM.CurrentOutBind;
            if (expose == ExposeMode.Full)
            {
                regBindRadio.IsChecked = !bindingVM.ShowShift;
                shiftBindRadio.IsChecked = bindingVM.ShowShift;
            }
            else
            {
                //topGrid.Visibility = Visibility.Collapsed;
                topGrid.ColumnDefinitions.RemoveAt(3);
                keyMouseTopTxt.Visibility = Visibility.Collapsed;
                macroOnLb.Visibility = Visibility.Collapsed;
                recordMacroBtn.Visibility = Visibility.Collapsed;
                mouseCanvas.Visibility = Visibility.Collapsed;
                bottomPanel.Visibility = Visibility.Collapsed;
                extrasSidePanel.Visibility = Visibility.Collapsed;
                mouseGridColumn.Width = new GridLength(0);
                //otherKeysMouseGrid.Columns = 2;
                Width = 950;
                Height = 300;
            }

            ChangeForCurrentAction();
        }

        private void OutConBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            //string name = button.Tag.ToString();
            var name = GetControlString(button);
            highlightLb.Content = name;

            var left = Canvas.GetLeft(button);
            var top = Canvas.GetTop(button);

            Canvas.SetLeft(highlightImg, left + button.Width / 2.0 - highlightImg.Height / 2.0);
            Canvas.SetTop(highlightImg, top + button.Height / 2.0 - highlightImg.Height / 2.0);

            Canvas.SetLeft(highlightLb, left + button.Width / 2.0 - highlightLb.ActualWidth / 2.0);
            Canvas.SetTop(highlightLb, top - 30);

            highlightImg.Visibility = Visibility.Visible;
            highlightLb.Visibility = Visibility.Visible;
        }

        private string GetControlString(Button button)
        {
            string result;
            if (bindingVM.Using360Mode)
            {
                var xboxcontrol = associatedBindings[button].Control;
                result = EnumDescriptionConverter.GetEnumDescription(xboxcontrol);
            }
            else
            {
                var xboxcontrol = associatedBindings[button].Control;
                result = Global.Ds4DefaultNames[xboxcontrol];
            }

            return result;
        }

        private void OutConBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightImg.Visibility = Visibility.Hidden;
            highlightLb.Visibility = Visibility.Hidden;
        }

        private void OutputKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var binding = bindingVM.ActionBinding;
            binding.OutputType = OutBinding.OutType.Key;
            if (associatedBindings.TryGetValue(button, out var bind))
            {
                binding.OutputType = OutBinding.OutType.Key;
                binding.OutKey = bind.OutKey;
            }

            Close();
        }

        private void OutputButtonBtn_Click(object sender, RoutedEventArgs e)
        {
            var binding = bindingVM.ActionBinding;
            var defaultControl = Global.DefaultButtonMapping[(int)binding.Input];
            var button = sender as Button;
            if (associatedBindings.TryGetValue(button, out var bind))
            {
                if (defaultControl == bind.Control && !binding.IsShift())
                {
                    binding.OutputType = OutBinding.OutType.Default;
                }
                else
                {
                    binding.OutputType = OutBinding.OutType.Button;
                    binding.Control = bind.Control;
                }
            }

            Close();
        }

        private void ChangeForCurrentAction()
        {
            var bind = bindingVM.ActionBinding;
            topOptsPanel.DataContext = bind;

            if (expose == ExposeMode.Full)
            {
                extrasGB.DataContext = bind;
                modePanel.DataContext = bind;
                shiftTriggerCombo.Visibility = bind.IsShift() ? Visibility.Visible : Visibility.Hidden;
                macroOnLb.DataContext = bind;
            }

            FindCurrentHighlightButton();
        }

        private void FindCurrentHighlightButton()
        {
            if (highlightBtn != null) highlightBtn.Background = SystemColors.ControlBrush;

            var binding = bindingVM.ActionBinding;
            if (binding.OutputType == OutBinding.OutType.Default)
            {
                var defaultBind = Global.DefaultButtonMapping[(int)binding.Input];
                if (!OutBinding.IsMouseRange(defaultBind))
                {
                    if (conBtnMap.TryGetValue(defaultBind, out var tempBtn))
                        OutConBtn_MouseEnter(tempBtn, null);
                    //tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                }
                else
                {
                    if (mouseBtnMap.TryGetValue(defaultBind, out var tempBtn))
                    {
                        tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                        highlightBtn = tempBtn;
                    }
                }
            }
            else if (binding.OutputType == OutBinding.OutType.Button)
            {
                if (!binding.IsMouse())
                {
                    if (conBtnMap.TryGetValue(binding.Control, out var tempBtn))
                        OutConBtn_MouseEnter(tempBtn, null);
                    //tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                }
                else
                {
                    if (mouseBtnMap.TryGetValue(binding.Control, out var tempBtn))
                    {
                        tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                        highlightBtn = tempBtn;
                    }
                }
            }
            else if (binding.OutputType == OutBinding.OutType.Key)
            {
                if (keyBtnMap.TryGetValue(binding.OutKey, out var tempBtn))
                {
                    tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                    highlightBtn = tempBtn;
                }
            }
        }

        private void InitInfoMaps()
        {
            foreach (var pair in associatedBindings)
            {
                var button = pair.Key;
                var binding = pair.Value;
                if (binding.OutputType == BindAssociation.OutType.Button)
                {
                    if (!binding.IsMouse())
                        conBtnMap.Add(binding.Control, button);
                    else
                        mouseBtnMap.Add(binding.Control, button);
                }
                else if (binding.OutputType == BindAssociation.OutType.Key)
                {
                    if (!keyBtnMap.ContainsKey(binding.OutKey)) keyBtnMap.Add(binding.OutKey, button);
                }
            }
        }

        private void InitButtonBindings()
        {
            associatedBindings.Add(aBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.A });
            aBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(bBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.B });
            bBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(xBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.X });
            xBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(yBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.Y });
            yBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lbBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LB });
            lbBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(ltBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LT });
            ltBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rbBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RB });
            rbBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rtBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RT });
            rtBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(backBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.Back });
            backBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(startBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.Start });
            startBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(guideBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.Guide });
            guideBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lsbBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LS });
            lsbBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lsuBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LYNeg });
            lsuBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lsrBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LXPos });
            lsrBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lsdBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LYPos });
            lsdBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(lslBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LXNeg });
            lslBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(dpadUBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.DpadUp });
            dpadUBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(dpadRBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.DpadRight });
            dpadRBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(dpadDBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.DpadDown });
            dpadDBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(dpadLBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.DpadLeft });
            dpadLBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rsbBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RS });
            rsbBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rsuBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RYNeg });
            rsuBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rsrBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RXPos });
            rsrBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rsdBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RYPos });
            rsdBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(rslBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RXNeg });
            rslBtn.Click += OutputButtonBtn_Click;

            associatedBindings.Add(touchpadClickBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.TouchpadClick });
            touchpadClickBtn.Click += OutputButtonBtn_Click;

            associatedBindings.Add(mouseUpBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.MouseUp });
            mouseUpBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseDownBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.MouseDown });
            mouseDownBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseLeftBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.MouseLeft });
            mouseLeftBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseRightBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.MouseRight });
            mouseRightBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseLBBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.LeftMouse });
            mouseLBBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseMBBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.MiddleMouse });
            mouseMBBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseRBBtn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.RightMouse });
            mouseRBBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouse4Btn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.FourthMouse });
            mouse4Btn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouse5Btn,
                new BindAssociation
                    { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.FifthMouse });
            mouse5Btn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseWheelUBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.WUP });
            mouseWheelUBtn.Click += OutputButtonBtn_Click;
            associatedBindings.Add(mouseWheelDBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Button, Control = X360ControlItem.WDOWN });
            mouseWheelDBtn.Click += OutputButtonBtn_Click;
        }

        private void InitKeyBindings()
        {
            associatedBindings.Add(escBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x1B });
            escBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f1Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x70 });
            f1Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f2Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x71 });
            f2Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f3Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x72 });
            f3Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f4Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x73 });
            f4Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f5Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x74 });
            f5Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f6Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x75 });
            f6Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f7Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x76 });
            f7Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f8Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x77 });
            f8Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f9Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x78 });
            f9Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f10Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x79 });
            f10Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f11Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x7A });
            f11Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(f12Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x7B });
            f12Btn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(oem3Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xC0 });
            oem3Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(oneBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x31 });
            oneBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(twoBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x32 });
            twoBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(threeBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x33 });
            threeBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(fourBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x34 });
            fourBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(fiveBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x35 });
            fiveBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(sixBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x36 });
            sixBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(sevenBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x37 });
            sevenBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(eightBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x38 });
            eightBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(nineBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x39 });
            nineBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(zeroBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x30 });
            zeroBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(minusBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBD });
            minusBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(equalBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBB });
            equalBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(bsBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x08 });
            bsBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(tabBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x09 });
            tabBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(qKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x51 });
            qKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(wKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x57 });
            wKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(eKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x45 });
            eKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x52 });
            rKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(tKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x54 });
            tKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(yKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x59 });
            yKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(uKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x55 });
            uKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(iKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x49 });
            iKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(oKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4F });
            oKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(pKey,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x50 });
            pKey.Click += OutputKeyBtn_Click;
            associatedBindings.Add(lbracketBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xDB });
            lbracketBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rbracketBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xDD });
            rbracketBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(bSlashBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xDC });
            bSlashBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(capsLBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x14 });
            capsLBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(aKeyBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x41 });
            aKeyBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(sBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x53 });
            sBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(dBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x44 });
            dBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(fBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x46 });
            fBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(gBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x47 });
            gBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(hBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x48 });
            hBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(jBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4A });
            jBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(kBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4B });
            kBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(lBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4C });
            lBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(semicolonBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBA });
            semicolonBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(aposBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xDE });
            aposBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(enterBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x0D });
            enterBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(lshiftBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x10 });
            lshiftBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(zBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x5A });
            zBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(xKeyBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x58 });
            xKeyBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(cBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x43 });
            cBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(vBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x56 });
            vBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(bKeyBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x42 });
            bKeyBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(nBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4E });
            nBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(mBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x4D });
            mBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(commaBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBC });
            commaBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(periodBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBE });
            periodBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(bslashBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xBF });
            bslashBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rshiftBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xA1 });
            rshiftBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(lctrlBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xA2 });
            lctrlBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(lWinBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x5B });
            lWinBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(laltBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x12 });
            laltBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(spaceBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x20 });
            spaceBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(raltBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xA5 });
            raltBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rwinBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x5C });
            rwinBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rctrlBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xA3 });
            rctrlBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(prtBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x2C });
            prtBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(sclBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x91 });
            sclBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(brkBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x13 });
            brkBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(insBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x2D });
            insBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(homeBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x24 });
            homeBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(pgupBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x21 });
            pgupBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(delBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x2E });
            delBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(endBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x23 });
            endBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(pgdwBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x22 });
            pgdwBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(uarrowBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x26 });
            uarrowBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(larrowBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x25 });
            larrowBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(darrowBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x28 });
            darrowBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(rarrowBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x27 });
            rarrowBtn.Click += OutputKeyBtn_Click;

            associatedBindings.Add(prevTrackBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xB1 });
            prevTrackBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(stopBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xB2 });
            stopBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(playBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xB3 });
            playBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(nextTrackBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xB0 });
            nextTrackBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(volupBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xAF });
            volupBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numlockBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x90 });
            numlockBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numdivideBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x6F });
            numdivideBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(nummultiBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x6A });
            nummultiBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numminusBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x6D });
            numminusBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(voldownBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xAE });
            voldownBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num7Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x67 });
            num7Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num8Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x68 });
            num8Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num9Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x69 });
            num9Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numplusBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x6B });
            numplusBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(volmuteBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0xAD });
            volmuteBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num4Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x64 });
            num4Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num5Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x65 });
            num5Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num6Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x66 });
            num6Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num1Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x61 });
            num1Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num2Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x62 });
            num2Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num3Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x63 });
            num3Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(num0Btn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x60 });
            num0Btn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numPeriodBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x6E });
            numPeriodBtn.Click += OutputKeyBtn_Click;
            associatedBindings.Add(numEnterBtn,
                new BindAssociation { OutputType = BindAssociation.OutType.Key, OutKey = 0x0D });
            numEnterBtn.Click += OutputKeyBtn_Click;
        }

        private void InitDS4Canvas()
        {
            var sourceConverter = new ImageSourceConverter();
            var temp = sourceConverter.ConvertFromString(
                    $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/{Application.Current.FindResource("DS4ConfigImg")}")
                as ImageSource;
            conImageBrush.ImageSource = temp;

            Canvas.SetLeft(aBtn, 442);
            Canvas.SetTop(aBtn, 148);
            Canvas.SetLeft(bBtn, 474);
            Canvas.SetTop(bBtn, 120);
            Canvas.SetLeft(xBtn, 408);
            Canvas.SetTop(xBtn, 116);
            Canvas.SetLeft(yBtn, 440);
            Canvas.SetTop(yBtn, 90);
            Canvas.SetLeft(lbBtn, 154);
            Canvas.SetTop(lbBtn, 24);
            lbBtn.Width = 46;
            lbBtn.Height = 20;

            Canvas.SetLeft(rbBtn, 428);
            Canvas.SetTop(rbBtn, 24);
            rbBtn.Width = 46;
            rbBtn.Height = 20;

            Canvas.SetLeft(ltBtn, 162);
            Canvas.SetTop(ltBtn, 6);
            ltBtn.Width = 46;
            ltBtn.Height = 20;

            Canvas.SetLeft(rtBtn, 428);
            Canvas.SetTop(rtBtn, 6);
            rtBtn.Width = 46;
            rtBtn.Height = 20;

            Canvas.SetLeft(backBtn, 218);
            Canvas.SetTop(backBtn, 76);
            Canvas.SetLeft(startBtn, 395);
            Canvas.SetTop(startBtn, 76);
            Canvas.SetLeft(guideBtn, 303);
            Canvas.SetTop(guideBtn, 162);

            Canvas.SetLeft(lsbBtn, 238);
            Canvas.SetTop(lsbBtn, 182);
            Canvas.SetLeft(lsuBtn, 230);
            Canvas.SetTop(lsuBtn, 160);
            lsuBtn.Width = 32;
            lsuBtn.Height = 16;

            Canvas.SetLeft(lsrBtn, 264);
            Canvas.SetTop(lsrBtn, 176);
            lsrBtn.Width = 16;
            lsrBtn.Height = 28;

            Canvas.SetLeft(lsdBtn, 232);
            Canvas.SetTop(lsdBtn, 202);
            lsdBtn.Width = 32;
            lsdBtn.Height = 16;

            Canvas.SetLeft(lslBtn, 216);
            Canvas.SetTop(lslBtn, 176);
            lslBtn.Width = 16;
            lslBtn.Height = 28;

            Canvas.SetLeft(rsbBtn, 377);
            Canvas.SetTop(rsbBtn, 184);
            Canvas.SetLeft(rsuBtn, 370);
            Canvas.SetTop(rsuBtn, 160);
            rsuBtn.Width = 32;
            rsuBtn.Height = 16;

            Canvas.SetLeft(rsrBtn, 400);
            Canvas.SetTop(rsrBtn, 176);
            rsrBtn.Width = 16;
            rsrBtn.Height = 28;

            Canvas.SetLeft(rsdBtn, 370);
            Canvas.SetTop(rsdBtn, 200);
            rsdBtn.Width = 32;
            rsdBtn.Height = 16;

            Canvas.SetLeft(rslBtn, 352);
            Canvas.SetTop(rslBtn, 176);
            rslBtn.Width = 16;
            rslBtn.Height = 28;

            Canvas.SetLeft(dpadUBtn, 170);
            Canvas.SetTop(dpadUBtn, 100);
            Canvas.SetLeft(dpadRBtn, 194);
            Canvas.SetTop(dpadRBtn, 112);
            Canvas.SetLeft(dpadDBtn, 170);
            Canvas.SetTop(dpadDBtn, 144);
            Canvas.SetLeft(dpadLBtn, 144);
            Canvas.SetTop(dpadLBtn, 112);

            touchpadClickBtn.Visibility = Visibility.Visible;
        }

        private void RegBindRadio_Click(object sender, RoutedEventArgs e)
        {
            if (regBindRadio.IsChecked == true)
            {
                bindingVM.ActionBinding = bindingVM.CurrentOutBind;
                ChangeForCurrentAction();
            }
        }

        private void ShiftBindRadio_Click(object sender, RoutedEventArgs e)
        {
            if (shiftBindRadio.IsChecked == true)
            {
                bindingVM.ActionBinding = bindingVM.ShiftOutBind;
                ChangeForCurrentAction();
            }
        }

        private void TestRumbleBtn_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = bindingVM.DeviceNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    if (!bindingVM.RumbleActive)
                    {
                        bindingVM.RumbleActive = true;
                        d.SetRumble((byte)Math.Min(255, bindingVM.ActionBinding.LightRumble),
                            (byte)Math.Min(255, bindingVM.ActionBinding.HeavyRumble));
                        testRumbleBtn.Content = Properties.Resources.StopText;
                    }
                    else
                    {
                        bindingVM.RumbleActive = false;
                        d.SetRumble(0, 0);
                        testRumbleBtn.Content = Properties.Resources.TestText;
                    }
                }
            }
        }

        private void ExtrasColorChoosebtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var actBind = bindingVM.ActionBinding;
            var tempcolor = actBind.ExtrasColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            bindingVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { bindingVM.UpdateForcedColor(color); };
            dialog.ShowDialog();
            bindingVM.EndForcedColor();
            actBind.UpdateExtrasColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            var actBind = bindingVM.ActionBinding;

            if (!actBind.ShiftBind)
            {
                actBind.OutputType = OutBinding.OutType.Default;
                actBind.Control = Global.DefaultButtonMapping[(int)actBind.Input];
            }
            else
            {
                actBind.OutputType = OutBinding.OutType.Default;
            }

            Close();
        }

        private void UnboundBtn_Click(object sender, RoutedEventArgs e)
        {
            var actBind = bindingVM.ActionBinding;
            actBind.OutputType = OutBinding.OutType.Button;
            actBind.Control = X360ControlItem.Unbound;
            Close();
        }

        private void RecordMacroBtn_Click(object sender, RoutedEventArgs e)
        {
            var box = new RecordBox(bindingVM.DeviceNum, bindingVM.Settings,
                bindingVM.ActionBinding.IsShift());
            box.Visibility = Visibility.Visible;
            mapBindingPanel.Visibility = Visibility.Collapsed;
            extrasGB.IsEnabled = false;
            fullPanel.Children.Add(box);
            box.Cancel += (sender2, args) =>
            {
                box.Visibility = Visibility.Collapsed;
                fullPanel.Children.Remove(box);
                box = null;
                mapBindingPanel.Visibility = Visibility.Visible;
                extrasGB.IsEnabled = true;
            };

            box.Save += (sender2, args) =>
            {
                box.Visibility = Visibility.Collapsed;
                fullPanel.Children.Remove(box);
                box = null;
                //mapBindingPanel.Visibility = Visibility.Visible;
                bindingVM.PopulateCurrentBinds();
                Close();
            };
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bindingVM.WriteBinds();
        }
    }
}