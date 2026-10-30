using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MathTrainer.View.Converters
{
    public class AssetUriToBitmapConverter : IValueConverter
    {
        //private static Bitmap? _defaultBitmap = null;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string uriString && !string.IsNullOrWhiteSpace(uriString))
            {
                try
                {
                    Uri uri = new Uri(uriString);
                    
                    if (AssetLoader.Exists(uri))
                    {
                        return new Bitmap(AssetLoader.Open(uri));
                    }
                    
                    //return new Bitmap(AssetLoader.Open(uri));
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[Converter] Falsche wert oder Typ: '{value}'");
                    System.Console.WriteLine($"Fehler beim Konvertieren URI {uriString} zu Bitmap: {ex.Message}");
                }
            }
            return new Bitmap(AssetLoader.Open(new Uri("avares://MathTrainer.View/Assets/Avatars/default_avatar.png")));
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}