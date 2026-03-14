using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.Converters;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Řídí logiku zobrazení, vyhledávání a filtrování transakcí v hlavním seznamu (historii).
    /// </summary>
    internal class TransactionListViewModel : BaseViewModel
    {
        private readonly DatabaseContext _dbContext;
        
        // Zdrojová data načtená z databáze a udržovaná v paměti
        private ObservableCollection<Transaction> _transactions = new ObservableCollection<Transaction>();
        
        // Obálka nad hrubými daty, která umožňuje řazení a filtrování v reálném čase,
        // aniž bychom měnili původní list Transactions (WPF standard pro DataGrid/ListView).
        private ICollectionView? _transactionsView;

        // Stavové proměnné pro aktuální nastavení filtrů
        private string _searchText = string.Empty;
        private string? _selectedFilterType; 
        private Category? _selectedFilterCategory;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;

        // Seznamy pro naplnění ComboBoxů ve filtru (typy a kategorie)
        private readonly ObservableCollection<Category> _availableCategories;
        private readonly ObservableCollection<string> _availableTypes;

        // Příkazy pro obsluhu tlačítek v UI
        private readonly ICommand _addTransactionCommand;
        private readonly ICommand _editTransactionCommand;
        private readonly ICommand _deleteTransactionCommand;
        private readonly ICommand _clearFilterCommand;

        // Veřejné vlastnosti pro datové vazby (Binding) v XAML
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions; 
            set => SetProperty(ref _transactions, value); 
        }

        // ICollectionView je to, co skutečně napojujeme na DataGrid/ListView v XAML
        // Umožňuje nám mít "živý" pohled na data, který se automaticky aktualizuje při změně filtru nebo řazení
        public ICollectionView? TransactionsView
        {
            get => _transactionsView; 
            set => SetProperty(ref _transactionsView, value); 
        }

        // Seznam kategorií pro ComboBox filtru. Obsahuje i umělou položku "Vše" pro zobrazení všech záznamů bez ohledu na kategorii
        public ObservableCollection<Category> AvailableCategories => _availableCategories;

        // Seznam typů pro ComboBox filtru. Obsahuje "Vše", "Příjem" a "Výdaj"
        public ObservableCollection<string> AvailableTypes => _availableTypes;

        // Vlastnosti navázané na prvky filtru v UI
        // Důležité: Při jakékoliv změně kritéria se okamžitě volá Refresh(), což vyvolá přepočítání zobrazených položek
        public string SearchText
        {
            get => _searchText; 
            set { if (SetProperty(ref _searchText, value)) TransactionsView?.Refresh(); }
        }

        // Vyber typu transakce pro filtraci
        // "Vše" znamená žádný filtr, jinak se filtruje podle zvoleného typu (Příjem/Výdaj)
        public string? SelectedFilterType
        {
            get => _selectedFilterType; 
            set { if (SetProperty(ref _selectedFilterType, value)) TransactionsView?.Refresh(); }
        }

        // Vyber nadřazené kategorie pro filtraci
        // Pokud je vybrána umělá kategorie "Vše" (Id = Guid.Empty), filtr se neaplikuje
        public Category? SelectedFilterCategory
        {
            get => _selectedFilterCategory; 
            set { if (SetProperty(ref _selectedFilterCategory, value)) TransactionsView?.Refresh(); }
        }

        // Filtr pro zobrazení transakcí pouze v určitém časovém rozpětí
        // Pokud je některá z hodnot null, filtr se neaplikuje
        public DateTime? DateFrom
        {
            get => _dateFrom; 
            set { if (SetProperty(ref _dateFrom, value)) TransactionsView?.Refresh(); }
        }

        // Filtr pro zobrazení transakcí pouze v určitém časovém rozpětí
        public DateTime? DateTo
        {
            get => _dateTo; 
            set { if (SetProperty(ref _dateTo, value)) TransactionsView?.Refresh(); }
        }

        // --- Příkazy svázané s UI tlačítky ---
        public ICommand AddTransactionCommand => _addTransactionCommand;
        public ICommand EditTransactionCommand => _editTransactionCommand;
        public ICommand DeleteTransactionCommand => _deleteTransactionCommand;
        public ICommand ClearFilterCommand => _clearFilterCommand;

        // Událost předávající signál pro otevření okna editoru transakcí
        public event Action<Transaction?>? RequestEditTransaction;

        /// <summary>
        /// Konstruktor načítá data z databáze, připravuje kolekce pro zobrazení a nastavuje výchozí hodnoty filtrů.
        /// </summary>
        /// <param name="dbContext"> Instance databáze pro načítání transakcí a kategorií. </param>
        public TransactionListViewModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _transactions = new ObservableCollection<Transaction>(_dbContext.Transactions);

            // Příprava seznamu kategorií pro ComboBox filtru včetně umělé položky "Vše"
            _availableCategories = new ObservableCollection<Category>();
            var allCategory = new Category { Id = Guid.Empty, Name = "Vše" };
            _availableCategories.Add(allCategory);
            
            foreach (var cat in _dbContext.Categories)
            {
                _availableCategories.Add(cat);
            }
            _selectedFilterCategory = allCategory; 

            _availableTypes = new ObservableCollection<string> { "Vše", "Příjem", "Výdaj" };
            _selectedFilterType = "Vše"; 

            // Výchozí rozsah filtru je nastaven na aktuální měsíc
            _dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _dateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                          DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            // Vytvoření ICollectionView filtru napojeného na naši metodu FilterTransactions
            _transactionsView = CollectionViewSource.GetDefaultView(_transactions);
            _transactionsView.Filter = FilterTransactions;
            
            // Základní řazení novějších záznamů nahoru
            _transactionsView.SortDescriptions.Add(new SortDescription(nameof(Transaction.Date), ListSortDirection.Descending));

            _addTransactionCommand = new RelayCommand(ExecuteAddTransaction);
            _editTransactionCommand = new RelayCommand<Transaction>(ExecuteEditTransaction);
            _deleteTransactionCommand = new RelayCommand<Transaction>(ExecuteDeleteTransaction);
            _clearFilterCommand = new RelayCommand(ExecuteClearFilter);
        }

        /// <summary>
        /// Hlavní logika pro filtrování transakcí. 
        /// Kontroluje každou položku proti aktuálním nastavením filtrů (text, typ, kategorie, datum) a rozhoduje, zda se má zobrazit.
        /// </summary>
        /// <param name="item"> Objekt, který se má zkontrolovat. Očekává se, že bude typu Transaction. </param>
        /// <returns> True, pokud položka splňuje všechna kritéria a měla by být zobrazena; False, pokud by měla být skryta. </returns>
        private bool FilterTransactions(object item)
        {
            if (item is Transaction transaction)
            {
                // 1. Textové vyhledávání v popisu
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    if (!transaction.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                // 2. Filtr typu (Příjem/Výdaj)
                if (SelectedFilterType != "Vše")
                {
                    TransactionType expectedType = SelectedFilterType == "Příjem" ? TransactionType.Income : TransactionType.Expense;
                    if (transaction.Type != expectedType)
                        return false;
                }

                // 3. Filtr dle zvolené nadřazené kategorie
                if (SelectedFilterCategory != null && SelectedFilterCategory.Id != Guid.Empty)
                {
                    if (transaction.CategoryId != SelectedFilterCategory.Id)
                        return false;
                }

                // 4. Filtr časového rozpětí
                if (DateFrom.HasValue && transaction.Date.Date < DateFrom.Value.Date)
                    return false;

                if (DateTo.HasValue && transaction.Date.Date > DateTo.Value.Date)
                    return false;

                // Splnilo všechny podmínky propusti - záznam zobrazit
                return true;
            }
            return false; // Skryté kvůli neodpovídajícímu typu entity (bezpečnostní chyták)
        }

        // --- Obsluha akcí (Commands) ---

        /// <summary>
        /// Resetuje všechny filtry na výchozí hodnoty, což způsobí zobrazení všech transakcí bez ohledu na text, typ, kategorii nebo datum.
        /// </summary>
        private void ExecuteClearFilter()
        {
            SearchText = string.Empty;
            SelectedFilterType = "Vše";
            SelectedFilterCategory = AvailableCategories.FirstOrDefault(c => c.Id == Guid.Empty);
            DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                          DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
        }

        /// <summary>
        /// Spouští událost pro otevření okna editoru transakcí v režimu
        /// </summary>
        private void ExecuteAddTransaction()
        {
            RequestEditTransaction?.Invoke(null);
        }

        /// <summary>
        /// Spouští událost pro otevření okna editoru transakcí s předanou transakcí pro editaci.
        /// </summary>
        /// <param name="transactionToEdit"> Transakce, kterou chceme editovat. Pokud je null, otevře se prázdný formulář pro vytvoření nové transakce. </param>
        private void ExecuteEditTransaction(Transaction? transactionToEdit)
        {
            if (transactionToEdit != null)
            {
                RequestEditTransaction?.Invoke(transactionToEdit);
            }
        }

        /// <summary>
        /// Zobrazí potvrzovací dialog pro smazání transakce. Pokud uživatel potvrdí, odstraní transakci z databáze i z kolekce pro zobrazení a uloží změny.
        /// </summary>
        /// <param name="transactionToDelete"> Transakce, kterou chceme smazat. Pokud je null, metoda se ukončí bez akce. </param>
        private void ExecuteDeleteTransaction(Transaction? transactionToDelete)
        {
            if (transactionToDelete == null) return;

            bool result = SpravaOsobnichFinanci.Views.CustomMessageBox.Show(
                $"Opravdu chcete smazat transakci '{transactionToDelete.Description}' ve výši {transactionToDelete.Amount:N2} {CurrencyConverter.CurrentSymbol}?",
                "Potvrzení smazání",
                null);

            if (result)
            {
                _dbContext.Transactions.Remove(transactionToDelete);
                Transactions.Remove(transactionToDelete);
                _dbContext.SaveData();
            }
        }
    }
}
