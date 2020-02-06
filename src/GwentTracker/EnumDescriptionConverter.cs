using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GwentTracker.Localization;

namespace GwentTracker
{
    public class EnumDescriptionConverter : IValueConverter
    {
        private readonly Translate _t;

        public EnumDescriptionConverter()
        {
            _t = new Translate();
        }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Enum e)
                return _t[e.GetDescription()];

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}