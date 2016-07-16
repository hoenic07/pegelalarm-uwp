using System;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Pegelalarm.Converters
{
    public class BooleanVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = (string)parameter == "invert";

            if (targetType == typeof(bool) && value is Visibility)
            {
                if (invert)
                {
                    return (Visibility)value == Visibility.Collapsed;
                }
                else
                {
                    return (Visibility)value == Visibility.Visible;
                }
            }
            if (targetType == typeof(Visibility) && value is bool)
            {
                if (invert)
                {
                    return (bool)value ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
