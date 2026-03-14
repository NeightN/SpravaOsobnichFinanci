using SpravaOsobnichFinanci.Converters;
using System;
using Xunit; 

namespace SpravaOsobnichFinanci.Tests.Converters
{
    /// <summary>
    /// Testuje formátování finančních hodnot v rozhraní za účelem prevence chyb zobrazování měn.
    /// </summary>
    public class CurrencyConverterTests
    {
        [Fact] 
        public void Convert_ValidDecimalAmount_ReturnsFormattedString()
        {
            // --- ARRANGE ---
            var converter = new CurrencyConverter();
            
            // Fixování statické vlastnosti používané v celé aplikaci pro zajištění izolovanosti testu
            CurrencyConverter.CurrentSymbol = "Kč"; 
            decimal amount = 1250.55m;
            
            // Očekáváme mezeru jako vizuální oddělovač (pevná mezera) tisíců a čárku pro desetinná místa 
            // - Pevná mezera má hexadecimální hodnotu 00A0
            string expectedResult = "1\u00A0250,55 Kč"; 

            // --- ACT ---
            // Nulové parametry pro targetType a Culture napodobují situaci, kdy WPF provádí DataBinding
#pragma warning disable CS8625 // Záměrné vypnutí kontroly null (WPF Binding Engin občas hodnoty null posílá)
            var result = converter.Convert(amount, typeof(string), null, null);
#pragma warning restore CS8625

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal(expectedResult, (string)result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // --- ARRANGE ---
            var converter = new CurrencyConverter();

            // --- ACT & ASSERT ---
            // ConvertBack není v aplikaci nikdy vyžadován, pokus o zpětný převod z XAMLu musí havarovat
            Assert.Throws<NotImplementedException>(() => 
            {
#pragma warning disable CS8625 
                converter.ConvertBack("1 250,55 Kč", typeof(decimal), null, null);
#pragma warning restore CS8625
            });
        }
    }
}
