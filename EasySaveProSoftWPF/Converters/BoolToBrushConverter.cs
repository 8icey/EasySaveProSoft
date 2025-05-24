using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

// converter
namespace EasySaveProSoft.WPF.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isValid = (bool)value;
            return isValid ? Brushes.Gray : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
