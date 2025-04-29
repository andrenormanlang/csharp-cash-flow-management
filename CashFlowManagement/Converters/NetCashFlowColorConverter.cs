using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CashFlowManagement.Converters
{
    public class NetCashFlowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                if (amount > 0) return new SolidColorBrush(Colors.Green);
                if (amount < 0) return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
