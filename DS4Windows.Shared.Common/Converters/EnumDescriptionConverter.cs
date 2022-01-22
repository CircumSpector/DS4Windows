using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DS4Windows.Shared.Common.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myEnum = (Enum)value;
            var description = GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        /// <summary>
        ///     Lookup and return the value of the Description attribute of an Enum item.
        /// </summary>
        /// <param name="enumObj">The <see cref="Enum"/> item to lookup.</param>
        /// <returns>The description string, if found. An Empty string otherwise.</returns>
        public static string GetEnumDescription(Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            var attributes = fieldInfo?.GetCustomAttributes(false);

            if (attributes is null)
                return string.Empty;

            if (attributes.Length == 0) return enumObj.ToString();

            var attribute = attributes[0] as DescriptionAttribute;

            return attribute?.Description ?? string.Empty;
        }
    }
}