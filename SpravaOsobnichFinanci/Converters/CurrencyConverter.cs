using System;
using System.Globalization;
using System.Windows.Data;

namespace SpravaOsobnichFinanci.Converters
{
    /// <summary>
    /// Slouží pro převod číselné hodnoty (decimal) z ViewModelu na naformátovaný textový řetězec v XAML pohledu.
    /// Připojuje přednastavenou značku měny a odděluje tisíce/desetinná místa podle českého formátu.
    /// </summary>
    internal class CurrencyConverter : IValueConverter
    {
        // Uchovává aktuální symbol měny, který se může měnit v nastavení aplikace.
        private static string _currentSymbol = "Kč";

        /// <summary>
        /// Statická vlastnost, pomocí které mohou Settings aktuálně měnit symbol měny napříč aplikací
        /// bez nutnosti posílat si zprávy.
        /// </summary>
        public static string CurrentSymbol 
        { 
            get => _currentSymbol; 
            set => _currentSymbol = value; 
        }

        /// <summary>
        /// Převádí decimalní hodnotu na formátovaný řetězec s měnovou značkou.
        /// </summary>
        /// <param name="value"> Hodnota, kterou chceme převést (očekáváme decimal). </param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty (obvykle string). </param>
        /// <param name="parameter"> Volitelný parametr pro další přizpůsobení (není využíván v této implementaci). </param>
        /// <param name="culture"> Kultura pro formátování (můžeme ji ignorovat, protože pevně nastavujeme české formátování). </param>
        /// <returns> Naformátovaný řetězec s částkou a měnovou značkou, nebo původní hodnota, pokud není decimal. </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ověříme, že vstupní hodnota je typu decimal, pokud ne, vrátíme ji beze změny (neformátovanou)
            if (value is decimal amount)
            {
                // Vynutíme české formátování čísel pro korektní oddělování mezerami a čárkou (např. 1 500,50 Kč)
                var cultureCz = new CultureInfo("cs-CZ");
                return $"{amount.ToString("N2", cultureCz)} {CurrentSymbol}";
            }

            // Pokud hodnota není decimal, vrátíme ji jako string bez formátování (nebo případně prázdný řetězec)
            return value;
        }

        // Zpětný převod z textu na decimal není implementován, protože není potřeba
        /// <summary>
        /// Zpětný převod není podporován, protože v našem scénáři není potřeba převádět text zpět na číslo.
        /// </summary>
        /// <param name="value"> Textový řetězec, který by se měl převést zpět na decimal (není využíván). </param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty (obvykle decimal). </param>
        /// <param name="parameter"> Volitelný parametr pro další přizpůsobení (není využíván v této implementaci). </param>
        /// <param name="culture"> Kultura pro formátování (můžeme ji ignorovat, protože tento převod není podporován). </param>
        /// <returns> Vzhledem k tomu, že tento převod není podporován, metoda vždy vyhodí výjimku. </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Zpětný převod textu na částku není podporován.");
        }
    }
}
