using System;
using System.Globalization;
using System.Windows.Data;
using SpravaOsobnichFinanci.Models;

namespace SpravaOsobnichFinanci.Converters
{
    /// <summary>
    /// Zajišťuje lokalizaci anglického výčtového typu (enum) TransactionType do českého jazyka pro potřeby uživatelského rozhraní.
    /// V kódu zůstává čitelná a typová struktura, zatímco uživatel vidí přeložený text.
    /// </summary>
    internal class TransactionTypeConverter : IValueConverter
    {
        /// <summary>
        /// Převádí hodnotu typu TransactionType na její českou reprezentaci pro zobrazení v UI.
        /// </summary>
        /// <param name="value"> Očekává se, že bude typu TransactionType. Pokud není, vrátí se jeho ToString() reprezentace.</param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty (obvykle string pro zobrazení). Tento parametr není v této implementaci využíván, protože výstup je vždy string.</param>
        /// <param name="parameter"> Volitelný parametr pro konverzi, který není v této implementaci využíván. Může být použit pro rozšíření funkcionality v budoucnu (např. pro specifikaci formátu nebo jazykové mutace).</param>
        /// <param name="culture"> Kultura, která může být využita pro lokalizaci (např. pro formátování dat nebo čísel). V této implementaci není využívána, protože převod je pevně daný a nezávislý na kultuře.</param>
        /// <returns> Českou reprezentaci typu TransactionType ("Příjem" pro Income, "Výdaj" pro Expense) nebo ToString() hodnoty, pokud není typu TransactionType.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                // Využití moderní syntaxe 'switch expression' pro efektivní mapování hodnot
                return type switch
                {
                    TransactionType.Income  => "Příjem",
                    TransactionType.Expense => "Výdaj",
                    
                    // Fallback větev (záchyt pro neočekávané hodnoty z budoucna), vrátí syrový název enumu
                    _                       => value.ToString() ?? string.Empty
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Zpětný převod z českého textu na výčtový typ TransactionType není implementován, protože v našem scénáři není potřeba převádět text zpět na enum.
        /// </summary>
        /// <param name="value"> Očekává se, že bude typu string, který reprezentuje českou hodnotu ("Příjem" nebo "Výdaj"). Nicméně, tento převod není podporován, takže tento parametr není využíván. </param>
        /// <param name="targetType"> Očekávaný typ cílové hodnoty (obvykle TransactionType), ale protože tento převod není podporován, tento parametr není využíván. </param>
        /// <param name="parameter"> Volitelný parametr pro konverzi, který není v této implementaci využíván. Může být použit pro rozšíření funkcionality v budoucnu (např. pro specifikaci formátu nebo jazykové mutace), ale v současné době není potřeba. </param>
        /// <param name="culture"> Kultura, která může být využita pro lokalizaci (např. pro formátování dat nebo čísel). V této implementaci není využívána, protože převod není podporován. </param>
        /// <returns> Vzhledem k tomu, že tento převod není podporován, metoda vždy vyhodí výjimku, aby bylo jasné, že tato operace není implementována. </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Zpětný překlad z českého textu na výčtový typ není v této aplikaci potřeba.");
        }
    }
}
