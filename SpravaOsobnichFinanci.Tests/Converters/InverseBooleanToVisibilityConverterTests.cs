using SpravaOsobnichFinanci.Converters;
using System.Windows;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.Converters
{
    /// <summary>
    /// Ověřuje správný převod Boolean logiky z ViewModelu na WPF Visibility enum. 
    /// Soustředí se na dodatečnou funkci s názvem 'Inverted', kterou nedisponuje nativní Convertor z knihovny Windows.
    /// </summary>
    public class InverseBooleanToVisibilityConverterTests
    {
        [Fact]
        public void Convert_TrueWithoutParameter_ReturnsVisible()
        {
            // --- ARRANGE ---
            var converter = new InverseBooleanToVisibilityConverter();
            bool valueToTest = true;

            // --- ACT ---
#pragma warning disable CS8625 
            var result = converter.Convert(valueToTest, typeof(Visibility), null, null);
#pragma warning restore CS8625

            // --- ASSERT ---
            // Standardní neočekávané chování (true = viditelný prvek v UI)
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_TrueWithInvertedParameter_ReturnsCollapsed()
        {
            // --- ARRANGE ---
            var converter = new InverseBooleanToVisibilityConverter();
            bool valueToTest = true;
            
            // XAML volání (CommandParameter) posílající klíčové slovo
            string parameter = "Inverted";

            // --- ACT ---
#pragma warning disable CS8625 
            var result = converter.Convert(valueToTest, typeof(Visibility), parameter, null);
#pragma warning restore CS8625

            // --- ASSERT ---
            // Očekáváme opačný Visibility enum, protože jsme převodnici modifikovali přes parametr
            Assert.Equal(Visibility.Collapsed, result); 
        }

        [Fact]
        public void Convert_NullOrInvalidType_ReturnsCollapsed()
        {
            // --- ARRANGE ---
            var converter = new InverseBooleanToVisibilityConverter();
            
            // Simulační scénář nesprávného typu propnutého z XAMLu (Zabezpečení proti pádu aplikace)
            string invalidValue = "Tohle není boolean logická hodnota";

            // --- ACT ---
#pragma warning disable CS8625 
            var result = converter.Convert(invalidValue, typeof(Visibility), null, null);
#pragma warning restore CS8625

            // --- ASSERT ---
            // Výchozí bezpečnostní stav (tzv. Fallback) pro neznámou věc na vstupu ukryje UI prvek z obrazovky
            Assert.Equal(Visibility.Collapsed, result); 
        }
    }
}
