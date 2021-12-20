using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Profiles.Schema;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4WinWPF.Translations;
using Microsoft.Win32;
using NonFormTimer = System.Timers.Timer;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        public delegate void CreatedProfileHandler(ProfileEditor sender, string profile);

        private bool controllerReadingsTabActive;
        private ProfileEntity currentProfileOLD;

        private readonly Dictionary<Button, ImageBrush> hoverImages =
            new();

        private readonly Dictionary<Button, int> hoverIndexes = new();
        private readonly Dictionary<Button, HoverImageInfo> hoverLocations = new();

        private readonly NonFormTimer inputTimer;

        private readonly MappingListViewModel mappingListVM;
        private readonly ProfileSettingsViewModel profileSettingsVM;
        private readonly Dictionary<int, Button> reverseHoverIndexes = new();
        private readonly SpecialActionsListViewModel specialActionsVM;

        private readonly ControlService rootHub;

        private readonly IAppSettingsService appSettings;

        /// <summary>
        ///     The currently edited <see cref="DS4WindowsProfile"/>.
        /// </summary>
        private readonly DS4WindowsProfile currentProfile;

        public ProfileEditor(DS4WindowsProfile profile, IAppSettingsService appSettings, ControlService service)
        {
            currentProfile = profile;
            this.appSettings = appSettings;
            rootHub = service;

            InitializeComponent();

            //DeviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(profile, appSettings, rootHub);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            mappingListVM = new MappingListViewModel(DeviceNum, profileSettingsVM.ContType);
            //specialActionsVM = new SpecialActionsListViewModel(device);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();
            PopulateReverseHoverIndexes();

            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignStickOuterBindAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        [Obsolete]
        public ProfileEditor(IAppSettingsService appSettings, ControlService service, int device)
        {
            this.appSettings = appSettings;
            rootHub = service;

            InitializeComponent();

            DeviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(appSettings, rootHub, device);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            mappingListVM = new MappingListViewModel(DeviceNum, profileSettingsVM.ContType);
            specialActionsVM = new SpecialActionsListViewModel(device);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();
            PopulateReverseHoverIndexes();

            AssignTiltAssociation();
            AssignSwipeAssociation();
            AssignTriggerFullPullAssociation();
            AssignStickOuterBindAssociation();
            AssignGyroSwipeAssociation();

            inputTimer = new NonFormTimer(100);
            inputTimer.Elapsed += InputDS4;
            SetupEvents();
        }

        public bool KeepSize { get; private set; }

        public int DeviceNum { get; private set; }

        public event EventHandler Closed;
        public event CreatedProfileHandler CreatedProfile;

        private void SetupEvents()
        {
            gyroOutModeCombo.SelectionChanged += GyroOutModeCombo_SelectionChanged;
            outConTypeCombo.SelectionChanged += OutConTypeCombo_SelectionChanged;
            mappingListBox.SelectionChanged += MappingListBox_SelectionChanged;
            Closed += ProfileEditor_Closed;

            // TODO: simplify!
            //profileSettingsVM.LSDeadZoneChanged += UpdateReadingsLsDeadZone;
            //profileSettingsVM.RSDeadZoneChanged += UpdateReadingsRsDeadZone;
            //profileSettingsVM.L2DeadZoneChanged += UpdateReadingsL2DeadZone;
            //profileSettingsVM.R2DeadZoneChanged += UpdateReadingsR2DeadZone;
            //profileSettingsVM.SXDeadZoneChanged += UpdateReadingsSXDeadZone;
            //profileSettingsVM.SZDeadZoneChanged += UpdateReadingsSZDeadZone;
        }

        private void UpdateReadingsSZDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;
        }

        private void UpdateReadingsSXDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
        }

        private void UpdateReadingsR2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
        }

        private void UpdateReadingsL2DeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
        }

        private void UpdateReadingsLsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadX = profileSettingsVM.LSDeadZone;
            conReadingsUserCon.LsDeadY = profileSettingsVM.LSDeadZone;
        }

        private void UpdateReadingsLsDeadZoneX(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadX = axialLSStickControl.AxialVM.DeadZoneX;
        }

        private void UpdateReadingsLsDeadZoneY(object sender, EventArgs e)
        {
            conReadingsUserCon.LsDeadY = axialLSStickControl.AxialVM.DeadZoneY;
        }

        private void UpdateReadingsRsDeadZone(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadX = profileSettingsVM.RSDeadZone;
            conReadingsUserCon.RsDeadY = profileSettingsVM.RSDeadZone;
        }

        private void UpdateReadingsRsDeadZoneX(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadX = axialRSStickControl.AxialVM.DeadZoneX;
        }

        private void UpdateReadingsRsDeadZoneY(object sender, EventArgs e)
        {
            conReadingsUserCon.RsDeadY = axialRSStickControl.AxialVM.DeadZoneY;
        }

        private void AssignTiltAssociation()
        {
            gyroZNLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroZNeg];
            gyroZPLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroZPos];
            gyroXNLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroXNeg];
            gyroXLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroXPos];
        }

        private void AssignSwipeAssociation()
        {
            swipeUpLb.DataContext = mappingListVM.ControlMap[DS4Controls.SwipeUp];
            swipeDownLb.DataContext = mappingListVM.ControlMap[DS4Controls.SwipeDown];
            swipeLeftLb.DataContext = mappingListVM.ControlMap[DS4Controls.SwipeLeft];
            swipeRightLb.DataContext = mappingListVM.ControlMap[DS4Controls.SwipeRight];
        }

        private void AssignTriggerFullPullAssociation()
        {
            l2FullPullLb.DataContext = mappingListVM.ControlMap[DS4Controls.L2FullPull];
            r2FullPullLb.DataContext = mappingListVM.ControlMap[DS4Controls.R2FullPull];
        }

        private void AssignStickOuterBindAssociation()
        {
            lsOuterBindLb.DataContext = mappingListVM.ControlMap[DS4Controls.LSOuter];
            rsOuterBindLb.DataContext = mappingListVM.ControlMap[DS4Controls.RSOuter];
        }

        private void AssignGyroSwipeAssociation()
        {
            gyroSwipeLeftLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeLeft];
            gyroSwipeRightLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeRight];
            gyroSwipeUpLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeUp];
            gyroSwipeDownLb.DataContext = mappingListVM.ControlMap[DS4Controls.GyroSwipeDown];
        }

        private void MappingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mappingListVM.SelectedIndex >= 0)
                if (reverseHoverIndexes.TryGetValue(mappingListVM.SelectedIndex, out var tempBtn))
                    InputControlHighlight(tempBtn);
        }

        private void PopulateReverseHoverIndexes()
        {
            foreach (var pair in hoverIndexes) reverseHoverIndexes.Add(pair.Value, pair.Key);
        }

        private void PopulateHoverIndexes()
        {
            hoverIndexes[crossConBtn] = 0;
            hoverIndexes[circleConBtn] = 1;
            hoverIndexes[squareConBtn] = 2;
            hoverIndexes[triangleConBtn] = 3;
            hoverIndexes[optionsConBtn] = 4;
            hoverIndexes[shareConBtn] = 5;
            hoverIndexes[upConBtn] = 6;
            hoverIndexes[downConBtn] = 7;
            hoverIndexes[leftConBtn] = 8;
            hoverIndexes[rightConBtn] = 9;
            hoverIndexes[guideConBtn] = 10;
            hoverIndexes[muteConBtn] = 11;
            hoverIndexes[l1ConBtn] = 12;
            hoverIndexes[r1ConBtn] = 13;
            hoverIndexes[l2ConBtn] = 14;
            hoverIndexes[r2ConBtn] = 15;
            hoverIndexes[l3ConBtn] = 16;
            hoverIndexes[r3ConBtn] = 17;

            hoverIndexes[leftTouchConBtn] = mappingListVM.ControlIndexMap[DS4Controls.TouchLeft]; // 21
            hoverIndexes[rightTouchConBtn] = mappingListVM.ControlIndexMap[DS4Controls.TouchRight]; // 22
            hoverIndexes[multiTouchConBtn] = mappingListVM.ControlIndexMap[DS4Controls.TouchMulti]; // 23
            hoverIndexes[topTouchConBtn] = mappingListVM.ControlIndexMap[DS4Controls.TouchUpper]; // 24

            hoverIndexes[lsuConBtn] = 25;
            hoverIndexes[lsdConBtn] = 26;
            hoverIndexes[lslConBtn] = 27;
            hoverIndexes[lsrConBtn] = 28;

            hoverIndexes[rsuConBtn] = 29;
            hoverIndexes[rsdConBtn] = 30;
            hoverIndexes[rslConBtn] = 31;
            hoverIndexes[rsrConBtn] = 32;

            hoverIndexes[gyroZNBtn] = 33;
            hoverIndexes[gyroZPBtn] = 34;
            hoverIndexes[gyroXNBtn] = 35;
            hoverIndexes[gyroXPBtn] = 36;

            hoverIndexes[swipeUpBtn] = 37;
            hoverIndexes[swipeDownBtn] = 38;
            hoverIndexes[swipeLeftBtn] = 39;
            hoverIndexes[swipeRightBtn] = 40;
        }

        private void PopulateHoverLocations()
        {
            hoverLocations[crossConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(crossConBtn), Canvas.GetTop(crossConBtn)),
                size = new Size(crossConBtn.Width, crossConBtn.Height)
            };
            hoverLocations[circleConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(circleConBtn), Canvas.GetTop(circleConBtn)),
                size = new Size(circleConBtn.Width, circleConBtn.Height)
            };
            hoverLocations[squareConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(squareConBtn), Canvas.GetTop(squareConBtn)),
                size = new Size(squareConBtn.Width, squareConBtn.Height)
            };
            hoverLocations[triangleConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(triangleConBtn), Canvas.GetTop(triangleConBtn)),
                size = new Size(triangleConBtn.Width, triangleConBtn.Height)
            };
            hoverLocations[l1ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l1ConBtn), Canvas.GetTop(l1ConBtn)),
                size = new Size(l1ConBtn.Width, l1ConBtn.Height)
            };
            hoverLocations[r1ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r1ConBtn), Canvas.GetTop(r1ConBtn)),
                size = new Size(r1ConBtn.Width, r1ConBtn.Height)
            };
            hoverLocations[l2ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l2ConBtn), Canvas.GetTop(l2ConBtn)),
                size = new Size(l2ConBtn.Width, l2ConBtn.Height)
            };
            hoverLocations[r2ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r2ConBtn), Canvas.GetTop(r2ConBtn)),
                size = new Size(r2ConBtn.Width, r2ConBtn.Height)
            };
            hoverLocations[shareConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(shareConBtn), Canvas.GetTop(shareConBtn)),
                size = new Size(shareConBtn.Width, shareConBtn.Height)
            };
            hoverLocations[optionsConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(optionsConBtn), Canvas.GetTop(optionsConBtn)),
                size = new Size(optionsConBtn.Width, optionsConBtn.Height)
            };
            hoverLocations[guideConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(guideConBtn), Canvas.GetTop(guideConBtn)),
                size = new Size(guideConBtn.Width, guideConBtn.Height)
            };
            hoverLocations[muteConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(muteConBtn), Canvas.GetTop(muteConBtn)),
                size = new Size(muteConBtn.Width, muteConBtn.Height)
            };

            hoverLocations[leftTouchConBtn] = new HoverImageInfo
            { point = new Point(144, 44), size = new Size(140, 98) };
            hoverLocations[multiTouchConBtn] = new HoverImageInfo
            { point = new Point(143, 42), size = new Size(158, 100) };
            hoverLocations[rightTouchConBtn] = new HoverImageInfo
            { point = new Point(156, 47), size = new Size(146, 94) };
            hoverLocations[topTouchConBtn] = new HoverImageInfo
            { point = new Point(155, 6), size = new Size(153, 114) };

            hoverLocations[l3ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsuConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsrConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsdConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lslConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                size = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };

            hoverLocations[r3ConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsuConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsrConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsdConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rslConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                size = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };

            hoverLocations[upConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(upConBtn), Canvas.GetTop(upConBtn)),
                size = new Size(upConBtn.Width, upConBtn.Height)
            };
            hoverLocations[rightConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(rightConBtn), Canvas.GetTop(rightConBtn)),
                size = new Size(rightConBtn.Width, rightConBtn.Height)
            };
            hoverLocations[downConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(downConBtn), Canvas.GetTop(downConBtn)),
                size = new Size(downConBtn.Width, downConBtn.Height)
            };
            hoverLocations[leftConBtn] = new HoverImageInfo
            {
                point = new Point(Canvas.GetLeft(leftConBtn), Canvas.GetTop(leftConBtn)),
                size = new Size(leftConBtn.Width, leftConBtn.Height)
            };
        }

        private void RemoveHoverBtnText()
        {
            crossConBtn.Content = string.Empty;
            circleConBtn.Content = string.Empty;
            squareConBtn.Content = string.Empty;
            triangleConBtn.Content = string.Empty;
            l1ConBtn.Content = string.Empty;
            r1ConBtn.Content = string.Empty;
            l2ConBtn.Content = string.Empty;
            r2ConBtn.Content = string.Empty;
            shareConBtn.Content = string.Empty;
            optionsConBtn.Content = string.Empty;
            guideConBtn.Content = string.Empty;
            muteConBtn.Content = string.Empty;
            leftTouchConBtn.Content = string.Empty;
            multiTouchConBtn.Content = string.Empty;
            rightTouchConBtn.Content = string.Empty;
            topTouchConBtn.Content = string.Empty;

            l3ConBtn.Content = string.Empty;
            lsuConBtn.Content = string.Empty;
            lsrConBtn.Content = string.Empty;
            lsdConBtn.Content = string.Empty;
            lslConBtn.Content = string.Empty;

            r3ConBtn.Content = string.Empty;
            rsuConBtn.Content = string.Empty;
            rsrConBtn.Content = string.Empty;
            rsdConBtn.Content = string.Empty;
            rslConBtn.Content = string.Empty;

            upConBtn.Content = string.Empty;
            rightConBtn.Content = string.Empty;
            downConBtn.Content = string.Empty;
            leftConBtn.Content = string.Empty;
        }

        private void PopulateHoverImages()
        {
            var sourceConverter = new ImageSourceConverter();

            var temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Cross.png") as ImageSource;
            var crossHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Circle.png") as ImageSource;
            var circleHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Square.png") as ImageSource;
            var squareHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Triangle.png") as ImageSource;
            var triangleHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_L1.png") as ImageSource;
            var l1Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_R1.png") as ImageSource;
            var r1Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_L2.png") as ImageSource;
            var l2Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_R2.png") as ImageSource;
            var r2Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Share.png") as ImageSource;
            var shareHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_options.png") as ImageSource;
            var optionsHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_PS.png") as ImageSource;
            var guideHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchLeft.png") as ImageSource;
            var leftTouchHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchMulti.png") as ImageSource;
            var multiTouchTouchHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchRight.png") as ImageSource;
            var rightTouchHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_TouchUpper.png") as ImageSource;
            var topTouchHover = new ImageBrush(temp);


            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            var l3Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            var lsuHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            var lsrHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            var lsdHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_LS.png") as ImageSource;
            var lslHover = new ImageBrush(temp);


            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            var r3Hover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            var rsuHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            var rsrHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            var rsdHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_RS.png") as ImageSource;
            var rslHover = new ImageBrush(temp);


            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Up.png") as ImageSource;
            var upHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Right.png") as ImageSource;
            var rightHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Down.png") as ImageSource;
            var downHover = new ImageBrush(temp);

            temp = sourceConverter.ConvertFromString(
                $"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/DS4-Config_Left.png") as ImageSource;
            var leftHover = new ImageBrush(temp);

            hoverImages[crossConBtn] = crossHover;
            hoverImages[circleConBtn] = circleHover;
            hoverImages[squareConBtn] = squareHover;
            hoverImages[triangleConBtn] = triangleHover;
            hoverImages[l1ConBtn] = l1Hover;
            hoverImages[r1ConBtn] = r1Hover;
            hoverImages[l2ConBtn] = l2Hover;
            hoverImages[r2ConBtn] = r2Hover;
            hoverImages[shareConBtn] = shareHover;
            hoverImages[optionsConBtn] = optionsHover;
            hoverImages[guideConBtn] = guideHover;
            hoverImages[muteConBtn] = guideHover;

            hoverImages[leftTouchConBtn] = leftTouchHover;
            hoverImages[multiTouchConBtn] = multiTouchTouchHover;
            hoverImages[rightTouchConBtn] = rightTouchHover;
            hoverImages[topTouchConBtn] = topTouchHover;
            hoverImages[l3ConBtn] = l3Hover;
            hoverImages[lsuConBtn] = lsuHover;
            hoverImages[lsrConBtn] = lsrHover;
            hoverImages[lsdConBtn] = lsdHover;
            hoverImages[lslConBtn] = lslHover;
            hoverImages[r3ConBtn] = r3Hover;
            hoverImages[rsuConBtn] = rsuHover;
            hoverImages[rsrConBtn] = rsrHover;
            hoverImages[rsdConBtn] = rsdHover;
            hoverImages[rslConBtn] = rslHover;

            hoverImages[upConBtn] = upHover;
            hoverImages[rightConBtn] = rightHover;
            hoverImages[downConBtn] = downHover;
            hoverImages[leftConBtn] = leftHover;
        }

        public async Task Reload(int device, ProfileEntity profile = null)
        {
            profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            lightbarRect.DataContext = null;

            DeviceNum = device;
            if (profile != null)
            {
                currentProfileOLD = profile;
                if (device == Global.TEST_PROFILE_INDEX)
                    Global.Instance.Config.ProfilePath[Global.TEST_PROFILE_INDEX] = profile.Name;

                await Global.Instance.LoadProfile(device, false, rootHub, false);
                profileNameTxt.Text = profile.Name;
                profileNameTxt.IsEnabled = false;
                applyBtn.IsEnabled = true;
            }
            else
            {
                currentProfileOLD = null;

                if (appSettings.Settings.AreProfilePresetsEnabled)
                {
                    var presetWin = new PresetOptionWindow(rootHub);
                    presetWin.SetupData(DeviceNum);
                    presetWin.ShowDialog();
                    // TODO: fix me!
                    /*
                    if (presetWin.Result == System.Windows.MessageBoxResult.Cancel)
                        Global.Instance.LoadBlankDevProfile(device, false, rootHub, false);
                    */
                }
            }

            ColorByBatteryPerCheck();

            if (device < Global.TEST_PROFILE_INDEX)
            {
                useControllerUD.Value = device + 1;
                conReadingsUserCon.UseDevice(device, device);
                contReadingsTab.IsEnabled = true;
            }
            else
            {
                useControllerUD.Value = 1;
                conReadingsUserCon.UseDevice(0, Global.TEST_PROFILE_INDEX);
                contReadingsTab.IsEnabled = true;
            }

            conReadingsUserCon.EnableControl(false);
            axialLSStickControl.UseDevice(currentProfile.LSModInfo);
            axialRSStickControl.UseDevice(currentProfile.RSModInfo);

            //specialActionsVM.LoadActions(currentProfileOLD == null);
            mappingListVM.UpdateMappings();
            profileSettingsVM.UpdateLateProperties();
            profileSettingsVM.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = profileSettingsVM;
            mappingListBox.DataContext = mappingListVM;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = profileSettingsVM;

            var lsMod = currentProfile.LSModInfo;
            if (lsMod.DZType == StickDeadZoneInfo.DeadZoneType.Radial)
            {
                conReadingsUserCon.LsDeadX = profileSettingsVM.LSDeadZone;
                conReadingsUserCon.LsDeadY = profileSettingsVM.LSDeadZone;
            }
            else if (lsMod.DZType == StickDeadZoneInfo.DeadZoneType.Axial)
            {
                conReadingsUserCon.LsDeadX = axialLSStickControl.AxialVM.DeadZoneX;
                conReadingsUserCon.LsDeadY = axialLSStickControl.AxialVM.DeadZoneY;
            }

            var rsMod = currentProfile.RSModInfo;
            if (rsMod.DZType == StickDeadZoneInfo.DeadZoneType.Radial)
            {
                conReadingsUserCon.RsDeadX = profileSettingsVM.RSDeadZone;
                conReadingsUserCon.RsDeadY = profileSettingsVM.RSDeadZone;
            }
            else if (rsMod.DZType == StickDeadZoneInfo.DeadZoneType.Axial)
            {
                conReadingsUserCon.RsDeadX = axialRSStickControl.AxialVM.DeadZoneX;
                conReadingsUserCon.RsDeadY = axialRSStickControl.AxialVM.DeadZoneY;
            }

            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;

            axialLSStickControl.AxialVM.DeadZoneXChanged += UpdateReadingsLsDeadZoneX;
            axialLSStickControl.AxialVM.DeadZoneYChanged += UpdateReadingsLsDeadZoneY;
            axialRSStickControl.AxialVM.DeadZoneXChanged += UpdateReadingsRsDeadZoneX;
            axialRSStickControl.AxialVM.DeadZoneYChanged += UpdateReadingsRsDeadZoneY;

            // Sort special action list by action name
            // TODO: fix me!
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(specialActionsVM.ActionCol);
            //view.SortDescriptions.Clear();
            //view.SortDescriptions.Add(new SortDescription("ActionName", ListSortDirection.Ascending));
            //view.Refresh();

            if (profileSettingsVM.UseControllerReadout) inputTimer.Start();
        }

        private void StopEditorBindings()
        {
            profileSettingsTabCon.DataContext = null;
            mappingListBox.DataContext = null;
            specialActionsTab.DataContext = null;
            lightbarRect.DataContext = null;
        }

        private void RefreshEditorBindings()
        {
            specialActionsVM.LoadActions(currentProfileOLD == null);
            mappingListVM.UpdateMappings();
            profileSettingsVM.UpdateLateProperties();
            profileSettingsVM.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroSwipeTrig(gyroSwipeTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroControlsTrig(gyroControlsTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = profileSettingsVM;
            mappingListBox.DataContext = mappingListVM;
            specialActionsTab.DataContext = specialActionsVM;
            lightbarRect.DataContext = profileSettingsVM;

            conReadingsUserCon.LsDeadX = profileSettingsVM.LSDeadZone;
            conReadingsUserCon.RsDeadX = profileSettingsVM.RSDeadZone;
            conReadingsUserCon.L2Dead = profileSettingsVM.L2DeadZone;
            conReadingsUserCon.R2Dead = profileSettingsVM.R2DeadZone;
            conReadingsUserCon.SixAxisXDead = profileSettingsVM.SXDeadZone;
            conReadingsUserCon.SixAxisZDead = profileSettingsVM.SZDeadZone;
        }

        private async void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, profileSettingsVM.FuncDevNum);
            Global.OutDevTypeTemp[DeviceNum] = OutContType.X360;
            await Global.Instance.LoadProfile(DeviceNum, false, rootHub);
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void HoverConBtn_Click(object sender, RoutedEventArgs e)
        {
            var mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void InputControlHighlight(Button control)
        {
            if (hoverImages.TryGetValue(control, out var tempBrush))
                picBoxHover.Source = tempBrush.ImageSource;
            //picBoxHover.Width = tempBrush.ImageSource.Width * .8;
            //picBoxHover.Height = tempBrush.ImageSource.Height * .8;
            //control.Background = tempBrush;
            //control.Background = new SolidColorBrush(Colors.Green);
            //control.Width = tempBrush.ImageSource.Width;
            //control.Height = tempBrush.ImageSource.Height;

            if (hoverLocations.TryGetValue(control, out var tempInfo))
            {
                Canvas.SetLeft(picBoxHover, tempInfo.point.X);
                Canvas.SetTop(picBoxHover, tempInfo.point.Y);
                picBoxHover.Width = tempInfo.size.Width;
                picBoxHover.Height = tempInfo.size.Height;
                picBoxHover.Stretch = Stretch.Fill;
                picBoxHover.Visibility = Visibility.Visible;
            }

            if (hoverIndexes.TryGetValue(control, out var tempIndex))
            {
                mappingListVM.SelectedIndex = tempIndex;
                mappingListBox.ScrollIntoView(mappingListBox.SelectedItem);
                var mapped = mappingListVM.Mappings[tempIndex];
                UpdateHighlightLabel(mapped);
            }
        }

        private void UpdateHighlightLabel(MappedControl mapped)
        {
            var display = $"{mapped.ControlName}: {mapped.MappingName}";
            if (mapped.HasShiftAction())
            {
                display += "\nShift: ";
                display += mapped.ShiftMappingName;
            }

            highlightControlDisplayLb.Content = display;
        }

        private void ContBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            var control = sender as Button;
            InputControlHighlight(control);
        }

        private void ContBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            //Button control = sender as Button;
            //control.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetLeft(picBoxHover, 0);
            Canvas.SetTop(picBoxHover, 0);
            picBoxHover.Visibility = Visibility.Hidden;
        }

        private void GyroOutModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var idx = gyroOutModeCombo.SelectedIndex;
            if (idx >= 0)
                if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                    rootHub.touchPad[DeviceNum]?.ResetToggleGyroModes();
        }

        private void SetLateProperties(bool fullSave = true)
        {
            currentProfile.BluetoothPollRate = profileSettingsVM.TempBTPollRateIndex;
            currentProfile.OutputDeviceType = profileSettingsVM.TempConType;
            if (fullSave) Global.OutDevTypeTemp[DeviceNum] = OutContType.X360;
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var saved = await ApplyProfileStep(false);
            if (saved) Closed?.Invoke(this, EventArgs.Empty);
        }

        [ConfigurationSystemComponent]
        private async Task<bool> ApplyProfileStep(bool fullSave = true)
        {
            var result = false;
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, profileSettingsVM.FuncDevNum);

            var temp = profileNameTxt.Text;
            if (!string.IsNullOrWhiteSpace(temp) &&
                temp.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
            {
                SetLateProperties(false);
                Global.Instance.Config.ProfilePath[DeviceNum] =
                    Global.Instance.Config.OlderProfilePath[DeviceNum] = temp;

                if (currentProfileOLD != null)
                    if (temp != currentProfileOLD.Name)
                    {
                        //File.Delete(DS4Windows.Global.RuntimeAppDataPath + @"\Profiles\" + currentProfile.Name + ".xml");
                        currentProfileOLD.DeleteFile();
                        currentProfileOLD.Name = temp;
                    }

                if (currentProfileOLD != null)
                {
                    await currentProfileOLD.SaveProfileAsync(DeviceNum);
                    currentProfileOLD.FireSaved();
                    result = true;
                }
                else
                {
                    var tempprof = Global.RuntimeAppDataPath + @"\Profiles\" + temp + ".xml";
                    if (!File.Exists(tempprof))
                    {
                        await Global.Instance.Config.SaveProfile(DeviceNum, temp);
                        CreatedProfile?.Invoke(this, temp);
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return result;
        }

        private void KeepSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            KeepSize = true;
            var c = new ImageSourceConverter();
            sizeImage.Source =
                c.ConvertFromString($"{Global.ASSEMBLY_RESOURCE_PREFIX}component/Resources/checked.png") as ImageSource;
        }

        public void Close()
        {
            if (profileSettingsVM.FuncDevNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.SetRumble(0, 0, profileSettingsVM.FuncDevNum);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ColorByBatteryPerCk_Click(object sender, RoutedEventArgs e)
        {
            ColorByBatteryPerCheck();
        }

        private void ColorByBatteryPerCheck()
        {
            var state = profileSettingsVM.ColorBatteryPercent;
            if (state)
            {
                colorGB.Header = Strings.Full;
                emptyColorGB.Visibility = Visibility.Visible;
            }
            else
            {
                colorGB.Header = Strings.Color;
                emptyColorGB.Visibility = Visibility.Hidden;
            }
        }

        private void FlashColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = profileSettingsVM.FlashColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { profileSettingsVM.UpdateForcedColor(color); };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateFlashColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void LowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = profileSettingsVM.LowColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { profileSettingsVM.UpdateForcedColor(color); };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateLowColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void HeavyRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    var rumbleActive = profileSettingsVM.HeavyRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.HeavyRumbleActive = true;
                        d.SetRumble(d.RightLightFastRumble,
                            (byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100));
                        heavyRumbleTestBtn.Content = Properties.Resources.StopHText;
                    }
                    else
                    {
                        profileSettingsVM.HeavyRumbleActive = false;
                        d.SetRumble(d.RightLightFastRumble, 0);
                        heavyRumbleTestBtn.Content = Properties.Resources.TestHText;
                    }
                }
            }
        }

        private void LightRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    var rumbleActive = profileSettingsVM.LightRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.LightRumbleActive = true;
                        d.SetRumble((byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100),
                            d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.StopLText;
                    }
                    else
                    {
                        profileSettingsVM.LightRumbleActive = false;
                        d.SetRumble(0, d.LeftHeavySlowRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.TestLText;
                    }
                }
            }
        }

        private void CustomEditorBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = btn.Tag.ToString();
            if (tag == "LS") LaunchCurveEditor(profileSettingsVM.LSCustomCurve);
            else if (tag == "RS") LaunchCurveEditor(profileSettingsVM.RSCustomCurve);
            else if (tag == "L2") LaunchCurveEditor(profileSettingsVM.L2CustomCurve);
            else if (tag == "R2") LaunchCurveEditor(profileSettingsVM.R2CustomCurve);
            else if (tag == "SX") LaunchCurveEditor(profileSettingsVM.SXCustomCurve);
            else if (tag == "SZ") LaunchCurveEditor(profileSettingsVM.SZCustomCurve);
        }

        private void LaunchCurveEditor(string customDefinition)
        {
            profileSettingsVM.LaunchCurveEditor(customDefinition);
        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Program (*.exe)|*.exe";
            dialog.Title = "Select Program";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == true) profileSettingsVM.UpdateLaunchProgram(dialog.FileName);
        }

        private void FrictionUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                rootHub.touchPad[DeviceNum]?.ResetTrackAccel(frictionUD.Value.GetValueOrDefault());
        }

        private void RainbowBtn_Click(object sender, RoutedEventArgs e)
        {
            var active = profileSettingsVM.Rainbow != 0.0;
            if (active)
            {
                profileSettingsVM.Rainbow = 0.0;
                colorByBatteryPerCk.Content = Properties.Resources.ColorByBattery;
                colorGB.IsEnabled = true;
                emptyColorGB.IsEnabled = true;
            }
            else
            {
                profileSettingsVM.Rainbow = 5.0;
                colorByBatteryPerCk.Content = Properties.Resources.DimByBattery;
                colorGB.IsEnabled = false;
                emptyColorGB.IsEnabled = false;
            }
        }

        private void ChargingColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = profileSettingsVM.ChargingColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { profileSettingsVM.UpdateForcedColor(color); };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateChargingColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void SteeringWheelEmulationCalibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.SASteeringWheelEmulationAxisIndex > 0)
            {
                var d = rootHub.DS4Controllers[profileSettingsVM.FuncDevNum];
                if (d != null)
                {
                    var origWheelCenterPoint = new System.Drawing.Point(d.wheelCenterPoint.X, d.wheelCenterPoint.Y);
                    var origWheel90DegPointLeft =
                        new System.Drawing.Point(d.wheel90DegPointLeft.X, d.wheel90DegPointLeft.Y);
                    var origWheel90DegPointRight =
                        new System.Drawing.Point(d.wheel90DegPointRight.X, d.wheel90DegPointRight.Y);

                    d.WheelRecalibrateActiveState = 1;

                    var result = MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrate}.\n\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction1}.\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction2}.\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction3}.\n\n" +
                                                 $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction}.\n",
                        Properties.Resources.SASteeringWheelEmulationCalibrate, MessageBoxButton.OKCancel,
                        MessageBoxImage.Information, MessageBoxResult.OK);

                    if (result == MessageBoxResult.OK)
                    {
                        // Accept new calibration values (State 3 is "Complete calibration" state)
                        d.WheelRecalibrateActiveState = 3;
                    }
                    else
                    {
                        // Cancel calibration and reset back to original calibration values
                        d.WheelRecalibrateActiveState = 4;

                        d.wheelFullTurnCount = 0;
                        d.wheelCenterPoint = origWheelCenterPoint;
                        d.wheel90DegPointLeft = origWheel90DegPointLeft;
                        d.wheel90DegPointRight = origWheel90DegPointRight;
                    }
                }
                else
                {
                    MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrateNoControllerError}.");
                }
            }
        }

        private void TouchDisInvertBtn_Click(object sender, RoutedEventArgs e)
        {
            touchDisInvertBtn.ContextMenu.IsOpen = true;
        }

        private void TouchDisInvertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            profileSettingsVM.UpdateTouchDisInvert(touchDisInvertBtn.ContextMenu);
        }

        private void GyroMouseTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroMouseTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseStickTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroMouseStickTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseStickTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroMouseStickTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseStickTrigBtn.ContextMenu.IsOpen = true;
        }

        private void OutConTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = outConTypeCombo.SelectedIndex;
            if (index >= 0) mappingListVM.UpdateMappingDevType(profileSettingsVM.TempConType);
        }

        private void NewActionBtn_Click(object sender, RoutedEventArgs e)
        {
            baseSpeActPanel.Visibility = Visibility.Collapsed;
            var profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
            var actEditor = new SpecialActionEditor(rootHub, DeviceNum, profList);
            specialActionDockPanel.Children.Add(actEditor);
            actEditor.Visibility = Visibility.Visible;
            actEditor.Cancel += (sender2, args) =>
            {
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;
            };
            actEditor.Saved += (sender2, actionName) =>
            {
                var action = Global.Instance.Config.GetAction(actionName);
                var newitem = specialActionsVM.CreateActionItem(action);
                newitem.Active = true;
                var lastIdx = specialActionsVM.ActionCol.Count;
                newitem.Index = lastIdx;
                specialActionsVM.ActionCol.Add(newitem);
                specialActionDockPanel.Children.Remove(actEditor);
                baseSpeActPanel.Visibility = Visibility.Visible;

                specialActionsVM.ExportEnabledActions();
                Global.Instance.Config.CacheExtraProfileInfo(profileSettingsVM.Device);
            };
        }

        private void EditActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                var item = specialActionsVM.CurrentSpecialActionItem;
                var currentIndex = item.Index;
                //int viewIndex = specialActionsVM.SpecialActionIndex;
                //int currentIndex = specialActionsVM.ActionCol[viewIndex].Index;
                //SpecialActionItem item = specialActionsVM.ActionCol[currentIndex];
                baseSpeActPanel.Visibility = Visibility.Collapsed;
                var profList = (Application.Current.MainWindow as MainWindow).ProfileListHolder;
                var actEditor = new SpecialActionEditor(rootHub, DeviceNum, profList, item.SpecialAction);
                specialActionDockPanel.Children.Add(actEditor);
                actEditor.Visibility = Visibility.Visible;
                actEditor.Cancel += (sender2, args) =>
                {
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                };
                actEditor.Saved += (sender2, actionName) =>
                {
                    var action = Global.Instance.Config.GetAction(actionName);
                    var newitem = specialActionsVM.CreateActionItem(action);
                    newitem.Active = item.Active;
                    newitem.Index = currentIndex;
                    specialActionsVM.ActionCol.RemoveAt(currentIndex);
                    specialActionsVM.ActionCol.Insert(currentIndex, newitem);
                    specialActionDockPanel.Children.Remove(actEditor);
                    baseSpeActPanel.Visibility = Visibility.Visible;
                    Global.Instance.Config.CacheExtraProfileInfo(profileSettingsVM.Device);
                };
            }
        }

        private void RemoveActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (specialActionsVM.SpecialActionIndex >= 0)
            {
                var item = specialActionsVM.CurrentSpecialActionItem;
                //int currentIndex = specialActionsVM.ActionCol[specialActionsVM.SpecialActionIndex].Index;
                //SpecialActionItem item = specialActionsVM.ActionCol[currentIndex];
                specialActionsVM.RemoveAction(item);
                Global.Instance.Config.CacheExtraProfileInfo(profileSettingsVM.Device);
            }
        }

        private void SpecialActionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            specialActionsVM.ExportEnabledActions();
        }

        private void Ds4LightbarColorBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = "Click the lightbar for color picker";
        }

        private void Ds4LightbarColorBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightControlDisplayLb.Content = string.Empty;
        }

        private void Ds4LightbarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            var tempcolor = profileSettingsVM.MainColor;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) => { profileSettingsVM.UpdateForcedColor(color); };
            dialog.ShowDialog();
            profileSettingsVM.EndForcedColor();
            profileSettingsVM.UpdateMainColor(dialog.colorPicker.SelectedColor.GetValueOrDefault());
        }

        private void InputDS4(object sender, ElapsedEventArgs e)
        {
            inputTimer.Stop();

            var activeWin = false;
            var tempDeviceNum = 0;
            Dispatcher.Invoke(() =>
            {
                activeWin = Application.Current.MainWindow.IsActive;
                tempDeviceNum = profileSettingsVM.FuncDevNum;
            });

            if (activeWin && profileSettingsVM.UseControllerReadout)
            {
                var index = -1;
                switch (ControlService.CurrentInstance.GetActiveInputControl(tempDeviceNum))
                {
                    case DS4Controls.None: break;
                    case DS4Controls.Cross:
                        index = 0;
                        break;
                    case DS4Controls.Circle:
                        index = 1;
                        break;
                    case DS4Controls.Square:
                        index = 2;
                        break;
                    case DS4Controls.Triangle:
                        index = 3;
                        break;
                    case DS4Controls.Options:
                        index = 4;
                        break;
                    case DS4Controls.Share:
                        index = 5;
                        break;
                    case DS4Controls.DpadUp:
                        index = 6;
                        break;
                    case DS4Controls.DpadDown:
                        index = 7;
                        break;
                    case DS4Controls.DpadLeft:
                        index = 8;
                        break;
                    case DS4Controls.DpadRight:
                        index = 9;
                        break;
                    case DS4Controls.PS:
                        index = 10;
                        break;
                    case DS4Controls.Mute:
                        index = 11;
                        break;
                    case DS4Controls.L1:
                        index = 12;
                        break;
                    case DS4Controls.R1:
                        index = 13;
                        break;
                    case DS4Controls.L2:
                        index = 14;
                        break;
                    case DS4Controls.R2:
                        index = 15;
                        break;
                    case DS4Controls.L3:
                        index = 16;
                        break;
                    case DS4Controls.R3:
                        index = 17;
                        break;
                    case DS4Controls.TouchLeft:
                        index = 18;
                        break;
                    case DS4Controls.TouchRight:
                        index = 19;
                        break;
                    case DS4Controls.TouchMulti:
                        index = 20;
                        break;
                    case DS4Controls.TouchUpper:
                        index = 21;
                        break;
                    case DS4Controls.LYNeg:
                        index = 22;
                        break;
                    case DS4Controls.LYPos:
                        index = 23;
                        break;
                    case DS4Controls.LXNeg:
                        index = 24;
                        break;
                    case DS4Controls.LXPos:
                        index = 25;
                        break;
                    case DS4Controls.RYNeg:
                        index = 26;
                        break;
                    case DS4Controls.RYPos:
                        index = 27;
                        break;
                    case DS4Controls.RXNeg:
                        index = 28;
                        break;
                    case DS4Controls.RXPos:
                        index = 29;
                        break;
                }

                if (index >= 0)
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        mappingListVM.SelectedIndex = index;
                        ShowControlBindingWindow();
                    }));
            }

            if (profileSettingsVM.UseControllerReadout) inputTimer.Start();
        }

        private void ProfileEditor_Closed(object sender, EventArgs e)
        {
            profileSettingsVM.UseControllerReadout = false;
            inputTimer.Stop();
            conReadingsUserCon.EnableControl(false);
            Global.Instance.Config.CacheExtraProfileInfo(profileSettingsVM.Device);
        }

        private void UseControllerReadoutCk_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.UseControllerReadout &&
                profileSettingsVM.Device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                inputTimer.Start();
            else
                inputTimer.Stop();
        }

        private void ShowControlBindingWindow()
        {
            var mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void MappingListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mappingListVM.SelectedIndex >= 0) ShowControlBindingWindow();
        }

        private void SidebarTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sidebarTabControl.SelectedItem == contReadingsTab)
            {
                controllerReadingsTabActive = true;
                conReadingsUserCon.EnableControl(true);
            }
            else if (controllerReadingsTabActive)
            {
                controllerReadingsTabActive = false;
                conReadingsUserCon.EnableControl(false);
            }
        }

        private void TiltControlsButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4Controls)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVM.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void SwipeControlsButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4Controls)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVM.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            UpdateHighlightLabel(mpControl);
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void ConBtn_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            var mpControl = mappingListVM.Mappings[mappingListVM.SelectedIndex];
            profileSettingsVM.PresetMenuUtil.SetHighlightControl(mpControl.Control);
            var cm = conCanvas.FindResource("presetMenu") as ContextMenu;
            var temp = cm.Items[0] as MenuItem;
            temp.Header = profileSettingsVM.PresetMenuUtil.PresetInputLabel;
            cm.PlacementTarget = btn;
            cm.IsOpen = true;
        }

        private void PresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var baseTag = Convert.ToInt32(item.Tag);
            var subTag = Convert.ToInt32(item.CommandParameter);
            if (baseTag >= 0 && subTag >= 0)
            {
                var controls =
                    profileSettingsVM.PresetMenuUtil.ModifySettingWithPreset(baseTag, subTag);
                foreach (var control in controls)
                {
                    var mpControl = mappingListVM.ControlMap[control];
                    mpControl.UpdateMappingName();
                }

                Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
                highlightControlDisplayLb.Content = string.Empty;
            }
        }

        private void PresetBtn_Click(object sender, RoutedEventArgs e)
        {
            sidebarTabControl.SelectedIndex = 0;

            var presetWin = new PresetOptionWindow(rootHub);
            presetWin.SetupData(DeviceNum);
            presetWin.ToPresetsScreen();
            presetWin.DelayPresetApply = true;
            presetWin.ShowDialog();

            if (presetWin.Result == System.Windows.MessageBoxResult.OK)
            {
                StopEditorBindings();
                presetWin.ApplyPreset();
                RefreshEditorBindings();
            }
        }

        private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            await ApplyProfileStep();
        }

        private void TriggerFullPullBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = Convert.ToInt32(btn.Tag);
            var ds4control = (DS4Controls)tag;
            if (ds4control == DS4Controls.None) return;

            //DS4ControlSettings setting = Global.getDS4CSetting(tag, ds4control);
            var mpControl = mappingListVM.ControlMap[ds4control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void GyroCalibration_Click(object sender, RoutedEventArgs e)
        {
            var deviceNum = profileSettingsVM.FuncDevNum;
            if (deviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var d = rootHub.DS4Controllers[deviceNum];
                d.SixAxis.ResetContinuousCalibration();
                if (d.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                {
                    var tempDev = rootHub.DS4Controllers[d.JointDeviceSlotNumber];
                    tempDev?.SixAxis.ResetContinuousCalibration();
                }
            }
        }

        private void GyroSwipeTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroSwipeTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroSwipeTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroSwipeTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroSwipeTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroSwipeControlsBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var control = (DS4Controls)Convert.ToInt32(btn.Tag);
            var mpControl = mappingListVM.ControlMap[control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private void GyroControlsTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroControlsTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroControlsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = gyroControlsTrigBtn.ContextMenu;
            var itemCount = menu.Items.Count;
            var alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroControlsTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void StickOuterBindButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tag = Convert.ToInt32(btn.Tag);
            var ds4control = (DS4Controls)tag;
            if (ds4control == DS4Controls.None) return;

            //DS4ControlSettings setting = Global.getDS4CSetting(tag, ds4control);
            var mpControl = mappingListVM.ControlMap[ds4control];
            var window = new BindingWindow(rootHub, DeviceNum, mpControl.Setting)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            mpControl.UpdateMappingName();
            Global.Instance.Config.CacheProfileCustomsFlags(profileSettingsVM.Device);
        }

        private class HoverImageInfo
        {
            public Point point;
            public Size size;
        }
    }

    public class ControlIndexCheck
    {
        public int TiltUp => (int)DS4Controls.GyroZNeg;
        public int TiltDown => (int)DS4Controls.GyroZPos;
        public int TiltLeft => (int)DS4Controls.GyroXPos;
        public int TiltRight => (int)DS4Controls.GyroXNeg;

        public int SwipeUp => (int)DS4Controls.SwipeUp;
        public int SwipeDown => (int)DS4Controls.SwipeDown;
        public int SwipeLeft => (int)DS4Controls.SwipeLeft;
        public int SwipeRight => (int)DS4Controls.SwipeRight;
        public int L2FullPull => (int)DS4Controls.L2FullPull;
        public int R2FullPull => (int)DS4Controls.R2FullPull;

        public int LSOuterBind => (int)DS4Controls.LSOuter;
        public int RSOuterBind => (int)DS4Controls.RSOuter;

        public int GyroSwipeLeft => (int)DS4Controls.GyroSwipeLeft;
        public int GyroSwipeRight => (int)DS4Controls.GyroSwipeRight;
        public int GyroSwipeUp => (int)DS4Controls.GyroSwipeUp;
        public int GyroSwipeDown => (int)DS4Controls.GyroSwipeDown;
    }
}