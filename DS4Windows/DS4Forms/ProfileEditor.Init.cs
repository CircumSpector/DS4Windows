using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Types;

namespace DS4WinWPF.DS4Forms
{
    public partial class ProfileEditor
    {
        /// <summary>
        ///     Links <see cref="Button"/>s to the corresponding <see cref="DS4ControlItem"/>.
        /// </summary>
        private void PopulateHoverIndexes()
        {
            hoverIndexes[crossConBtn] = DS4ControlItem.Cross;
            hoverIndexes[circleConBtn] = DS4ControlItem.Circle;
            hoverIndexes[squareConBtn] = DS4ControlItem.Square;
            hoverIndexes[triangleConBtn] = DS4ControlItem.Triangle;
            hoverIndexes[optionsConBtn] = DS4ControlItem.Options;
            hoverIndexes[shareConBtn] = DS4ControlItem.Share;
            hoverIndexes[upConBtn] = DS4ControlItem.DpadUp;
            hoverIndexes[downConBtn] = DS4ControlItem.DpadDown;
            hoverIndexes[leftConBtn] = DS4ControlItem.DpadLeft;
            hoverIndexes[rightConBtn] = DS4ControlItem.DpadRight;
            hoverIndexes[guideConBtn] = DS4ControlItem.PS;
            hoverIndexes[muteConBtn] = DS4ControlItem.Mute;
            hoverIndexes[l1ConBtn] = DS4ControlItem.L1;
            hoverIndexes[r1ConBtn] = DS4ControlItem.R1;
            hoverIndexes[l2ConBtn] = DS4ControlItem.L2;
            hoverIndexes[r2ConBtn] = DS4ControlItem.R2;
            hoverIndexes[l3ConBtn] = DS4ControlItem.L3;
            hoverIndexes[r3ConBtn] = DS4ControlItem.R3;

            hoverIndexes[leftTouchConBtn] = DS4ControlItem.TouchLeft;
            hoverIndexes[rightTouchConBtn] = DS4ControlItem.TouchRight;
            hoverIndexes[multiTouchConBtn] = DS4ControlItem.TouchMulti;
            hoverIndexes[topTouchConBtn] = DS4ControlItem.TouchUpper;

            hoverIndexes[lsuConBtn] = DS4ControlItem.LYPos;
            hoverIndexes[lsdConBtn] = DS4ControlItem.LYNeg;
            hoverIndexes[lslConBtn] = DS4ControlItem.LXNeg;
            hoverIndexes[lsrConBtn] = DS4ControlItem.LXPos;

            hoverIndexes[rsuConBtn] = DS4ControlItem.RYPos;
            hoverIndexes[rsdConBtn] = DS4ControlItem.RYNeg;
            hoverIndexes[rslConBtn] = DS4ControlItem.RXNeg;
            hoverIndexes[rsrConBtn] = DS4ControlItem.RYPos;

            hoverIndexes[gyroZNBtn] = DS4ControlItem.GyroZNeg;
            hoverIndexes[gyroZPBtn] = DS4ControlItem.GyroZPos;
            hoverIndexes[gyroXNBtn] = DS4ControlItem.GyroXNeg;
            hoverIndexes[gyroXPBtn] = DS4ControlItem.GyroXPos;

            hoverIndexes[swipeUpBtn] = DS4ControlItem.SwipeUp;
            hoverIndexes[swipeDownBtn] = DS4ControlItem.SwipeDown;
            hoverIndexes[swipeLeftBtn] = DS4ControlItem.SwipeLeft;
            hoverIndexes[swipeRightBtn] = DS4ControlItem.SwipeRight;
        }

        /// <summary>
        ///     Links <see cref="Button"/>s to the corresponding <see cref="HoverImageInfo"/>.
        /// </summary>
        private void PopulateHoverLocations()
        {
            hoverLocations[crossConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(crossConBtn), Canvas.GetTop(crossConBtn)),
                Dimensions = new Size(crossConBtn.Width, crossConBtn.Height)
            };
            hoverLocations[circleConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(circleConBtn), Canvas.GetTop(circleConBtn)),
                Dimensions = new Size(circleConBtn.Width, circleConBtn.Height)
            };
            hoverLocations[squareConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(squareConBtn), Canvas.GetTop(squareConBtn)),
                Dimensions = new Size(squareConBtn.Width, squareConBtn.Height)
            };
            hoverLocations[triangleConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(triangleConBtn), Canvas.GetTop(triangleConBtn)),
                Dimensions = new Size(triangleConBtn.Width, triangleConBtn.Height)
            };
            hoverLocations[l1ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l1ConBtn), Canvas.GetTop(l1ConBtn)),
                Dimensions = new Size(l1ConBtn.Width, l1ConBtn.Height)
            };
            hoverLocations[r1ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r1ConBtn), Canvas.GetTop(r1ConBtn)),
                Dimensions = new Size(r1ConBtn.Width, r1ConBtn.Height)
            };
            hoverLocations[l2ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l2ConBtn), Canvas.GetTop(l2ConBtn)),
                Dimensions = new Size(l2ConBtn.Width, l2ConBtn.Height)
            };
            hoverLocations[r2ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r2ConBtn), Canvas.GetTop(r2ConBtn)),
                Dimensions = new Size(r2ConBtn.Width, r2ConBtn.Height)
            };
            hoverLocations[shareConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(shareConBtn), Canvas.GetTop(shareConBtn)),
                Dimensions = new Size(shareConBtn.Width, shareConBtn.Height)
            };
            hoverLocations[optionsConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(optionsConBtn), Canvas.GetTop(optionsConBtn)),
                Dimensions = new Size(optionsConBtn.Width, optionsConBtn.Height)
            };
            hoverLocations[guideConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(guideConBtn), Canvas.GetTop(guideConBtn)),
                Dimensions = new Size(guideConBtn.Width, guideConBtn.Height)
            };
            hoverLocations[muteConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(muteConBtn), Canvas.GetTop(muteConBtn)),
                Dimensions = new Size(muteConBtn.Width, muteConBtn.Height)
            };

            hoverLocations[leftTouchConBtn] = new HoverImageInfo
                { Coordinates = new Point(144, 44), Dimensions = new Size(140, 98) };
            hoverLocations[multiTouchConBtn] = new HoverImageInfo
                { Coordinates = new Point(143, 42), Dimensions = new Size(158, 100) };
            hoverLocations[rightTouchConBtn] = new HoverImageInfo
                { Coordinates = new Point(156, 47), Dimensions = new Size(146, 94) };
            hoverLocations[topTouchConBtn] = new HoverImageInfo
                { Coordinates = new Point(155, 6), Dimensions = new Size(153, 114) };

            hoverLocations[l3ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                Dimensions = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsuConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                Dimensions = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsrConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                Dimensions = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lsdConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                Dimensions = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };
            hoverLocations[lslConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(l3ConBtn), Canvas.GetTop(l3ConBtn)),
                Dimensions = new Size(l3ConBtn.Width, l3ConBtn.Height)
            };

            hoverLocations[r3ConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                Dimensions = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsuConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                Dimensions = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsrConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                Dimensions = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rsdConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                Dimensions = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };
            hoverLocations[rslConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(r3ConBtn), Canvas.GetTop(r3ConBtn)),
                Dimensions = new Size(r3ConBtn.Width, r3ConBtn.Height)
            };

            hoverLocations[upConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(upConBtn), Canvas.GetTop(upConBtn)),
                Dimensions = new Size(upConBtn.Width, upConBtn.Height)
            };
            hoverLocations[rightConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(rightConBtn), Canvas.GetTop(rightConBtn)),
                Dimensions = new Size(rightConBtn.Width, rightConBtn.Height)
            };
            hoverLocations[downConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(downConBtn), Canvas.GetTop(downConBtn)),
                Dimensions = new Size(downConBtn.Width, downConBtn.Height)
            };
            hoverLocations[leftConBtn] = new HoverImageInfo
            {
                Coordinates = new Point(Canvas.GetLeft(leftConBtn), Canvas.GetTop(leftConBtn)),
                Dimensions = new Size(leftConBtn.Width, leftConBtn.Height)
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

        /// <summary>
        ///     Links <see cref="Button"/>s to the corresponding <see cref="ImageBrush"/>.
        /// </summary>
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
    }
}