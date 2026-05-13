using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;
using System;

namespace PIA.Converters
{
    public class StockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int stock = (int)value;

            if (stock <= 10)
                return new SolidColorBrush(Colors.Red);

            if (stock <= 20)
                return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 180, 0));

            return new SolidColorBrush(Microsoft.UI.Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StockWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int stock = (int)value;

            if (stock <= 20)
                return Microsoft.UI.Text.FontWeights.Bold;

            return Microsoft.UI.Text.FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}