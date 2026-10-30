using Avalonia.Data.Converters;
using MathTrainer.Model.Enums;
using System;
using System.Globalization;

namespace MathTrainer.View.Converters
{
    public class RequestStatusToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is RequestStatus status)
            {
                switch (status)
                {
                    case RequestStatus.Pendung:
                        return "⏳ Noch nicht ausgegeben";
                    case RequestStatus.Issued:
                        return "✅ Ausgegeben";
                    default:
                        return status.ToString();
                }
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
}