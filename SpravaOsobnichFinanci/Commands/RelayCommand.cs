using System;
using System.Windows.Input;

namespace SpravaOsobnichFinanci.Commands
{
    /// <summary>
    /// Zprostředkovává volání metod (Commands) z uživatelského rozhraní (Views) do logiky (ViewModels).
    /// Jde o standardní implementaci rozhraní ICommand pro návrhový vzor MVVM ve WPF.
    /// </summary>
    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Konstruktor pro příkazy, které nemají omezující podmínku (vždy enabled).
        /// </summary>
        /// <param name="execute"> Metoda z ViewModelu, která se má vykonat při spuštění příkazu (např. metoda pro přidání kategorie).</param>
        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Konstruktor pro příkazy, které mohou být enabled/disabled na základě logiky ve ViewModelu.
        /// </summary>
        /// <param name="execute"> Metoda z ViewModelu, která se má vykonat při spuštění příkazu (např. metoda pro smazání kategorie).</param>
        /// <param name="canExecute"> Metoda z ViewModelu, která vrací bool určující, zda je příkaz aktuálně povolen (enabled) nebo zakázán (disabled).
        /// <exception cref="ArgumentNullException"></exception>
        public RelayCommand(Action execute, Func<bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Událost, která upozorní rozhraní (např. Tlačítko), že má přehodnotit svůj stav (zda je enabled/disabled).
        /// Napojení na CommandManager.RequerySuggested říká samotnému WPF, aby tuto kontrolu provádělo automaticky 
        /// při interakci uživatele s aplikací (např. po kliknutí myší, stisku klávesy na klávesnici nebo změně focusu).
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Zjistí, zda je možné příkaz spustit (pokud ViewModel definuje omezující podmínku).
        /// </summary>
        /// <param name="parameter"> Nevyužívaný parametr pro ne-generickou verzi příkazu, ale musí být přítomen kvůli podpisu metody z rozhraní ICommand.</param>
        /// <returns> True, pokud je příkaz povolen (enabled), nebo False, pokud je zakázán (disabled).</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Fyzicky provede akci, kterou příkaz reprezentuje.
        /// </summary>
        /// <param name="parameter"> Nevyužívaný parametr pro ne-generickou verzi příkazu, ale musí být přítomen kvůli podpisu metody z rozhraní ICommand.</param>
        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// Generická verze vztahující se k příkazům, které z pohledu přijímají nějaký parametr
    /// (typicky předáno v průběhu bindingu v XAML pomocí atributu CommandParameter).
    /// </summary>
    /// <typeparam name="T">Očekávaný typ parametru posílaného z UI (např. typ kategorie ke smazání)</typeparam>
    internal class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// Konstruktor pro příkazy s parametrem, které nemají omezující podmínku (vždy enabled).
        /// </summary>
        /// <param name="execute"> Metoda z ViewModelu, která se má vykonat při spuštění příkazu (např. metoda pro smazání kategorie, která očekává jako parametr instanci kategorie).</param>
        public RelayCommand(Action<T?> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// Konstruktor pro příkazy s parametrem, které mohou být enabled/disabled na základě logiky ve ViewModelu.
        /// </summary>
        /// <param name="execute"> Metoda z ViewModelu, která se má vykonat při spuštění příkazu (např. metoda pro smazání kategorie, která očekává jako parametr instanci kategorie).</param>
        /// <param name="canExecute"> Metoda z ViewModelu, která vrací bool určující, zda je příkaz aktuálně povolen (enabled) nebo zakázán (disabled) na základě hodnoty parametru (např. může být zakázán, pokud je předaný parametr null).</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Událost, která upozorní rozhraní (např. Tlačítko), že má přehodnotit svůj stav (zda je enabled/disabled).
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Zjistí, zda je možné příkaz spustit (pokud ViewModel definuje omezující podmínku) na základě hodnoty předaného parametru.
        /// </summary>
        /// <param name="parameter"> Parametr posílaný z UI (např. instance kategorie, kterou chceme smazat). Může být null, pokud není předán žádný parametr nebo pokud je explicitně nastaven na null v XAML.</param>
        /// <returns> True, pokud je příkaz povolen (enabled) na základě logiky v canExecute, nebo False, pokud je zakázán (disabled).</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T?)parameter);
        }

        /// <summary>
        /// Fyzicky provede akci, kterou příkaz reprezentuje, s využitím předaného parametru.
        /// </summary>
        /// <param name="parameter"> Parametr posílaný z UI (např. instance kategorie, kterou chceme smazat). Může být null, pokud není předán žádný parametr nebo pokud je explicitně nastaven na null v XAML.</param>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }
    }
}
