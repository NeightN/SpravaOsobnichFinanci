using System.Windows;
using System.Windows.Input;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Models;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Nejvyšší řídící vrstva aplikační logiky (tzv. Root ViewModel). 
    /// Zajišťuje držení hlavní databázové instance (Singleton princip) a řídí kompletní navigaci.
    /// Využívá přístup "ViewModel-First", kdy se UI Views automaticky překreslují dle toho, který ViewModel je právě aktivní.
    /// </summary>
    internal class MainViewModel : BaseViewModel
    {
        // Sdílený kontext databáze, který se předává všem podřízeným obrazovkám
        private readonly DatabaseContext _dbContext;

        // --- Stavové proměnné pro navigaci ---
        private BaseViewModel? _currentViewModel;
        private string _currentViewName = string.Empty;
        
        // --- Globální příkazy menu ---
        private readonly ICommand _navigateToDashboardCommand;
        private readonly ICommand _navigateToTransactionsCommand;
        private readonly ICommand _navigateToCategoriesCommand;
        private readonly ICommand _navigateToSettingsCommand;
        private readonly ICommand _exitApplicationCommand;

        /// <summary>
        /// Aktuálně načtená obrazovka (DataTemplate ve View se postará o vyrenderování správného UserControlu).
        /// </summary>
        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel; 
            set => SetProperty(ref _currentViewModel, value); 
        }

        /// <summary>
        /// Indikátor aktuálně vybrané položky v menu (kvůli zvýraznění tlačítek v bočním panelu).
        /// </summary>
        public string CurrentViewName
        {
            get => _currentViewName; 
            set => SetProperty(ref _currentViewName, value); 
        }

        // --- Zveřejnění příkazů pro Binding z XAMLu ---
        public ICommand NavigateToDashboardCommand => _navigateToDashboardCommand;
        public ICommand NavigateToTransactionsCommand => _navigateToTransactionsCommand;
        public ICommand NavigateToCategoriesCommand => _navigateToCategoriesCommand;
        public ICommand NavigateToSettingsCommand => _navigateToSettingsCommand;
        public ICommand ExitApplicationCommand => _exitApplicationCommand;

        /// <summary>
        /// Konstruktor, který inicializuje hlavní databázový kontext a nastaví výchozí obrazovku (Dashboard).
        /// </summary>
        public MainViewModel()
        {
            _dbContext = new DatabaseContext();

            // Uložení měny do statického UI konvertoru hned při startu aplikace
            SpravaOsobnichFinanci.Converters.CurrencyConverter.CurrentSymbol = _dbContext.Settings.CurrencySymbol;

            _navigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);
            _navigateToTransactionsCommand = new RelayCommand(ExecuteNavigateToTransactions);
            _navigateToCategoriesCommand = new RelayCommand(ExecuteNavigateToCategories);
            _navigateToSettingsCommand = new RelayCommand(ExecuteNavigateToSettings);
            _exitApplicationCommand = new RelayCommand(ExecuteExitApplication);

            // Výchozí spouštěcí obrazovka
            ExecuteNavigateToDashboard();
        }

        /// <summary>
        /// Metoda pro přepnutí na Dashboard. Nastaví název aktuální obrazovky a vytvoří nový instance DashboardViewModel, který se načte do hlavní oblasti.
        /// </summary>
        private void ExecuteNavigateToDashboard()
        {
            CurrentViewName = "Dashboard"; 
            CurrentViewModel = new DashboardViewModel(_dbContext);
        }

        /// <summary>
        /// Metoda pro přepnutí na obrazovku s transakcemi. Nastaví název aktuální obrazovky a vytvoří nový instance TransactionListViewModel, který se načte do hlavní oblasti.
        /// </summary>
        private void ExecuteNavigateToTransactions()
        {
            CurrentViewName = "Transactions"; 
            var listViewModel = new TransactionListViewModel(_dbContext);
            
            // Registrace k odposlechu události pro případ, že uživatel v seznamu klikne na "Upravit"
            listViewModel.RequestEditTransaction += OnRequestEditTransaction;
            CurrentViewModel = listViewModel;
        }

        /// <summary>
        /// Metoda, která se spustí, když uživatel z TransactionListViewModelu požádá o editaci konkrétní transakce (nebo vytvoření nové).
        /// </summary>
        /// <param name="transaction"> Pokud je null, znamená to, že uživatel chce vytvořit novou transakci. Pokud není null, jedná se o úpravu existující transakce. </param>
        private void OnRequestEditTransaction(Transaction? transaction)
        {
            var editorViewModel = new TransactionEditorViewModel(_dbContext, transaction);
            // Zajištění toho, že jakmile se editor zavře/uloží, celá obrazovka se vrátí zpět na seznam
            editorViewModel.RequestClose += OnEditorRequestClose;
            CurrentViewModel = editorViewModel;
        }

        /// <summary>
        /// Metoda, která se spustí, když TransactionEditorViewModel vyšle požadavek na zavření (po uložení nebo zrušení).
        /// </summary>
        private void OnEditorRequestClose()
        {
            // Zrušením editoru se aplikace navrací zpět k výpisu tabulky
            ExecuteNavigateToTransactions();
        }

        /// <summary>
        /// Metoda pro přepnutí na obrazovku s kategoriemi. 
        /// Nastaví název aktuální obrazovky a vytvoří nový instance CategoryListViewModel, který se načte do hlavní oblasti.
        /// </summary>
        private void ExecuteNavigateToCategories()
        {
            CurrentViewName = "Categories"; 
            var categoryListViewModel = new CategoryListViewModel(_dbContext);
            categoryListViewModel.RequestEditCategory += OnRequestEditCategory;
            CurrentViewModel = categoryListViewModel;
        }

        /// <summary>
        /// Metoda, která se spustí, když uživatel z CategoryListViewModelu požádá o editaci konkrétní kategorie (nebo vytvoření nové).
        /// </summary>
        /// <param name="category"></param>
        private void OnRequestEditCategory(Category? category)
        {
            var editorViewModel = new CategoryEditorViewModel(_dbContext, category);
            editorViewModel.RequestClose += OnCategoryEditorRequestClose;
            CurrentViewModel = editorViewModel;
        }

        /// <summary>
        /// Metoda, která se spustí, když CategoryEditorViewModel vyšle požadavek na zavření (po uložení nebo zrušení).
        /// </summary>
        private void OnCategoryEditorRequestClose()
        {
            ExecuteNavigateToCategories();
        }

        /// <summary>
        /// Metoda pro přepnutí na obrazovku s nastavením. Nastaví název aktuální obrazovky a vytvoří nový instance SettingsViewModel, který se načte do hlavní oblasti.
        /// </summary>
        private void ExecuteNavigateToSettings()
        {
            CurrentViewName = "Settings"; 
            CurrentViewModel = new SettingsViewModel(_dbContext);
        }

        /// <summary>
        /// Metoda pro ukončení aplikace. Před samotným ukončením se pokusí provést poslední zápis do SQLite, aby nedošlo ke ztrátě dat.
        /// </summary>
        private void ExecuteExitApplication()
        {
            // Bezpečnostní mechanismus - před násilným ukončením programu se pokusíme provést poslední zápis do SQLite
            _dbContext.SaveData();
            Application.Current.Shutdown();
        }
    }
}