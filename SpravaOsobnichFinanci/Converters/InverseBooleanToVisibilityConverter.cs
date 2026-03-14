using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpravaOsobnichFinanci.Converters
{
    /// <summary>
    /// Univerzální konvertor, který propojuje logický typ boolean s enumrátorem Visibility (viditelností prvku ve WPF).
    /// Standardní WPF sice poskytuje BooleanToVisibilityConverter, ale tento konvertor navíc podporuje pomocí 
    /// Command parametru inverzi (pokud je true -> Visibility bude Collapsed, nikoliv Visible).
    /// </summary>
    internal class InverseBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Převádí logickou hodnotu (boolean) na hodnotu typu Visibility, přičemž podporuje volitelnou inverzi pomocí parametru.
        /// </summary>
        /// <param name="value"> Očekáváme logickou hodnotu (bool), která reprezentuje určitý stav (např. zda je něco načítáno). </param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty, který by měl být typu Visibility. </param>
        /// <param name="parameter"> Volitelný parametr, který pokud obsahuje klíčové slovo 'Inverted', způsobí, že logika viditelnosti bude opačná (true -> Collapsed, false -> Visible). </param>
        /// <param name="culture"> Kultura pro případné lokalizace (není využívána v této implementaci, ale je součástí podpisu metody). </param>
        /// <returns> Hodnota typu Visibility, která je Visible, pokud je boolean true (nebo false, pokud je použita inverze), a Collapsed v opačném případě. Pokud obdržíme neznámý typ nebo null, vrátíme Collapsed pro bezpečné skrytí prvku. </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Očekáváme logickou hodnotu zastupující určitý stav (např. IsLoading)
            if (value is bool boolValue)
            {
                // Pokud z front-endu (XAML) pošleme klíčové slovo 'Inverted', 
                // otáčíme klasickou logiku viditelnosti.
                if (parameter as string == "Inverted")
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                
                // Běžné (nenegované) chování
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Pojistka – pokud obdržíme neznámý typ nebo null, raději UI prvek skryjeme
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Zpětný převod z Visibility na boolean není pro tento konvertor implementován, protože v našem scénáři není potřeba převádět viditelnost zpět na logickou hodnotu.
        /// </summary>
        /// <param name="value"> Očekáváme hodnotu typu Visibility, kterou bychom teoreticky mohli převést zpět na boolean (není využíváno). </param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty, který by měl být typu boolean. </param>
        /// <param name="parameter"> Volitelný parametr pro další přizpůsobení (není využíván v této implementaci). </param>
        /// <param name="culture"> Kultura pro formátování (můžeme ji ignorovat, protože tento převod není podporován). </param>
        /// <returns> Vzhledem k tomu, že tento převod není podporován, metoda vždy vyhodí výjimku. </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Zpětný převod z Visibility na boolean není pro tento případ vyžadován.");
        }
    }
}
