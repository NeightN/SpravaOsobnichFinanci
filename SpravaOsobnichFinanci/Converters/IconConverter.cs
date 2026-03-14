using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace SpravaOsobnichFinanci.Converters
{
    /// <summary>
    /// Převádí textový klíč ikony (uložený v databázi) na konkrétní Unicode znak.
    /// Tento znak se následně v UI vykreslí pomocí speciálního fontu (Material Design Icons) jako vektorová ikona.
    /// </summary>
    internal class IconConverter : IValueConverter
    {
        // Slovník mapující klíče ikon na jejich odpovídající Unicode znaky
        private static readonly Dictionary<string, string> _icons = new()
        {
            ["Default"] = char.ConvertFromUtf32(0xF04F9),       // Tag
            ["Food"] = char.ConvertFromUtf32(0xF0A70),          // Silverware
            ["Home"] = char.ConvertFromUtf32(0xF02DC),          // Home
            ["Car"] = char.ConvertFromUtf32(0xF010B),           // Car
            ["Entertainment"] = char.ConvertFromUtf32(0xF0B82), // Gamepad
            ["Health"] = char.ConvertFromUtf32(0xF02D1),        // HeartPulse
            ["Shopping"] = char.ConvertFromUtf32(0xF0110),      // Cart
            ["Money"] = char.ConvertFromUtf32(0xF0114),         // Cash
            ["Bank"] = char.ConvertFromUtf32(0xF0070),          // Bank
            ["Clothes"] = char.ConvertFromUtf32(0xF053F),       // T-Shirt
            ["Pets"] = char.ConvertFromUtf32(0xF03E9),          // Paw
            ["Education"] = char.ConvertFromUtf32(0xF1180),     // School
            ["Travel"] = char.ConvertFromUtf32(0xF001D),        // Airplane
            ["Gifts"] = char.ConvertFromUtf32(0xF02A1),         // Gift
            ["Coffee"] = char.ConvertFromUtf32(0xF0176),        // Coffee
            ["Phone"] = char.ConvertFromUtf32(0xF011C),         // Cellphone
            ["Internet"] = char.ConvertFromUtf32(0xF05A9)       // Wifi
        };

        ///<summary>
        /// Převádí textový klíč ikony na odpovídající Unicode znak pro zobrazení v UI.
        /// Pokud klíč není nalezen nebo je null, vrací výchozí ikonu (Tag).
        /// </summary>
        /// <param name="value">Textový klíč ikony (např. "Food", "Home")</param>
        /// <param name="targetType">Očekávaný typ cíle (v tomto případě string)</param>
        /// <param name="parameter">Volitelný parametr (nepoužívá se)</param>
        /// <param name="culture">Kultura pro lokalizaci (nepoužívá se)</param>
        /// <returns>Unicode znak reprezentující ikonu pro zobrazení v UI</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Pokud získáme platný klíč ikony, pokusíme se spárovat ho se slovníkem
            if (value is string iconKey && !string.IsNullOrEmpty(iconKey))
            {
                return _icons.TryGetValue(iconKey, out string? iconChar) ? iconChar : _icons["Default"];
            }
            
            // Záchranná varianta (tzv. Fallback) - pokud klíč chybí nebo je null, vrátíme výchozí ikonu lístku
            return _icons["Default"];
        }

        /// <summary>
        /// Zpětný převod není implementován, protože není potřeba převádět zpět z ikony na klíč.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Binding do UI je zde čistě jednosměrný. Kliknutím na ikonu uživatel její kód nemění.
            throw new NotImplementedException("Zpětný převod ze znaku ikony na klíč není podporován.");
        }
    }
}
