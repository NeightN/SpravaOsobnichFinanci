using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Abstraktní základní třída tvořící páteř pro všechny ViewModely v aplikaci.
    /// Zajišťuje mechanismus pro oznamování změn dat z logické vrstvy do XAML pohledů.
    /// </summary>
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        // Klíčová událost WPF binding engine, která informuje uživatelské rozhraní o nutnosti překreslení.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sestaví a odešle notifikaci o změně vlastnosti.
        /// Atribut [CallerMemberName] zbavuje vývojáře nutnosti psát název volající vlastnosti ručně ("magic strings").
        /// </summary>
        /// <param name="propertyName">Automaticky předaný řetězec odpovídající názvu veřejné vlastnosti</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Zapouzdřuje validaci, přiřazení nové hodnoty a následnou notifikaci do jediné opakovaně využitelné metody.
        /// Zásadní pro optimalizaci: Zabraňuje zbytečnému spouštění notifikací v případech, kdy k fyzické změně hodnoty vůbec nedošlo.
        /// </summary>
        /// <typeparam name="T">Datový typ vlastnosti</typeparam>
        /// <param name="field">Odkaz přímo do paměti na privátní pole dané vlastnosti</param>
        /// <param name="value">Nově přiřazovaná hodnota</param>
        /// <param name="propertyName">Název vlastnosti (doplněn kompilátorem v čase běhu)</param>
        /// <returns>True, pokud byla hodnota reálně přepsána a UI upozorněno, jinak False.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            // Kontrola equality přes výchozí comparer zabraňuje problému u hodnotových i referenčních typů zároveň
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false; // Hodnota se nezměnila, nevyvoláváme náročnou překreslovací událost
            }

            // Aplikace nové hodnoty rovnou do paměťového místa
            field = value;
            OnPropertyChanged(propertyName);
            
            return true;
        }
    }
}
