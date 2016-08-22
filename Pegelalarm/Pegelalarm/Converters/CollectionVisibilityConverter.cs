using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Pegelalarm.Converters
{
    public class CollectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = (parameter as string) == "invert";
            if (value == null || (value is IEnumerable && (value as IEnumerable).Cast<object>().Count() == 0)) return invert ? Visibility.Visible : Visibility.Collapsed;
            return invert ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
