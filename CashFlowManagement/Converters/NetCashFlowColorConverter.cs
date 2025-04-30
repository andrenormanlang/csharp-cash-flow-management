using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CashFlowManagement.Converters
{
    /// <summary>
    /// Converts a decimal value representing net cash flow to a corresponding color brush.
    /// Green for positive values, red for negative values, and black for zero.
    /// </summary>
    public class NetCashFlowColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a decimal value to a SolidColorBrush based on whether the value is positive, negative, or zero.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <param name="targetType">The type of the target property.</param>
        /// <param name="parameter">Optional conversion parameter. Not used in this implementation.</param>
        /// <param name="culture">The culture to use for the conversion.</param>
        /// <returns>A SolidColorBrush representing the appropriate color for the value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                if (amount > 0) return new SolidColorBrush(Colors.Green);
                if (amount < 0) return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// Not implemented as conversion back to decimal is not needed.
        /// </summary>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
