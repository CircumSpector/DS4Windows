using DS4Windows.Shared.Common.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DS4Windows.Client.Modules.Profiles.Converters
{
    public class BezierCurveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                var bezierCurve = (BezierCurve)value;
                return bezierCurve.AsString;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                var bezierCurve = new BezierCurve();
                bezierCurve.InitBezierCurve((string)value, BezierCurve.AxisType.LSRS, true);
                return bezierCurve;
            }
        }
    }
}
