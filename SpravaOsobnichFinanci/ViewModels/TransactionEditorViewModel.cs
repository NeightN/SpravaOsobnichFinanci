using System;
using System.Collections.ObjectModel;
using System.Globalization; 
using System.Linq;
using System.Windows.Input;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Models;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Řídí logiku formuláře pro vytváření nové a úpravu stávající finanční transakce.
    /// Zajišťuje robustní validaci uživatelských vstupů (zejména textového zadávání částek).
    /// </summary>
    internal class TransactionEditorViewModel : BaseViewModel
    {
        private readonly DatabaseContext _dbContext;
        private readonly Transaction? _originalTransaction;

        private DateTime _date;
        private string _description = string.Empty;
        private string _amountInput = string.Empty;
        private TransactionType _type;
        private Category? _selectedCategory;
        private string _typeString = "Výdaj";

        private readonly ObservableCollection<string> _availableTypes;
        private readonly ObservableCollection<Category> _availableCategories;

        private readonly ICommand _saveCommand;
        private readonly ICommand _cancelCommand;

        public DateTime Date
        {
            get => _date; 
            set => SetProperty(ref _date, value); 
        }

        public string Description
        {
            get => _description; 
            set => SetProperty(ref _description, value); 
        }

        /// <summary>
        /// Textové pole zachytávající uživatelský vstup částky z GUI.
        /// Ihned na úrovni vlastnosti řeší UX problém s mícháním čárek a teček (např. při psaní z numerické klávesnice), 
        /// tím že je nahrazuje za platný český oddělovač (čárku).
        /// </summary>
        public string AmountInput
        {
            get => _amountInput; 
            set 
            { 
                string safeValue = value?.Replace('.', ',') ?? string.Empty;
                SetProperty(ref _amountInput, safeValue); 
            }
        }

        public TransactionType Type
        {
            get => _type; 
            set => SetProperty(ref _type, value); 
        }

        /// <summary>
        /// Vybraná kategorie z ComboBoxu. Její přítomnost je klíčová pro validaci a povolení uložení transakce.
        /// </summary>
        public Category? SelectedCategory
        {
            get => _selectedCategory; 
            set => SetProperty(ref _selectedCategory, value); 
        }

        public string TypeString
        {
            get => _typeString; 
            set => SetProperty(ref _typeString, value); 
        }

        public ObservableCollection<string> AvailableTypes => _availableTypes;

        public ObservableCollection<Category> AvailableCategories => _availableCategories;

        public ICommand SaveCommand => _saveCommand;
        
        public ICommand CancelCommand => _cancelCommand;

        /// <summary>
        /// Pošle požadavek na zrušení dialogového okna zpět hlavní navigaci ve chvíli, kdy akce skončila.
        /// </summary>
        public event Action? RequestClose;

        /// <summary>
        /// Konstruktor pro inicializaci ViewModelu. Pokud je předána existující transakce, naplní formulář jejími daty pro editaci.
        /// </summary>
        /// <param name="dbContext">Instance pro přístup k datům, potřebná pro načtení kategorií a uložení změn.</param>
        /// <param name="transaction">Volitelný parametr, pokud je poskytnut, znamená, že formulář bude sloužit k úprav
        public TransactionEditorViewModel(DatabaseContext dbContext, Transaction? transaction = null)
        {
            _dbContext = dbContext;
            _originalTransaction = transaction;

            _availableTypes = new ObservableCollection<string> { "Příjem", "Výdaj" };
            _availableCategories = new ObservableCollection<Category>(_dbContext.Categories);

            _saveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            _cancelCommand = new RelayCommand(ExecuteCancel);

            // Úprava stávající transakce - vyplníme hodnoty z DB objektu
            if (_originalTransaction != null)
            {
                Date = _originalTransaction.Date;
                Description = _originalTransaction.Description;
                
                AmountInput = _originalTransaction.Amount.ToString(new CultureInfo("cs-CZ"));
                
                Type = _originalTransaction.Type;
                TypeString = _originalTransaction.Type == TransactionType.Income ? "Příjem" : "Výdaj";
                SelectedCategory = AvailableCategories.FirstOrDefault(c => c.Id == _originalTransaction.CategoryId);
            }
            // Založení zcela nové transakce
            else
            {
                Date = DateTime.Now;
                Type = TransactionType.Expense; // Výdaj je statisticky nejčastější úkon
                TypeString = "Výdaj";
                SelectedCategory = AvailableCategories.FirstOrDefault();
            }
        }

        /// <summary>
        /// Pokusí se bezpečně převést nečistý textový vstup z pole Částka na číslo (Decimal).
        /// </summary>
        private decimal GetParsedAmount()
        {
            if (decimal.TryParse(AmountInput, NumberStyles.Any, new CultureInfo("cs-CZ"), out decimal result))
            {
                return result;
            }
            return 0; // Vrací nulu, pokud je vstup zcela nesmyslný text
        }

        /// <summary>
        /// Určuje, zda jsou splněny podmínky pro
        /// </summary>
        /// <returns>True, pokud je částka validní a kategorie je vybraná; jinak false.</returns>
        private bool CanExecuteSave()
        {
            // Tlačítko pro spuštění bude aktivní jen v případě, že máme vybranou kategorii a zadaná částka je větší než 0
            return GetParsedAmount() > 0 && SelectedCategory != null;
        }

        /// <summary>
        /// Provádí uložení nové nebo upravené transakce do databáze. Pokud se jedná o novou transakci, vytvoří nový objekt a přidá ho do kontextu.
        /// </summary>
        private void ExecuteSave()
        {
            TransactionType resolvedType = TypeString == "Příjem" ? TransactionType.Income : TransactionType.Expense;
            decimal parsedAmount = GetParsedAmount();

            if (_originalTransaction == null)
            {
                // Vystavění nové finanční entity
                var newTransaction = new Transaction
                {
                    Date = this.Date,
                    Description = this.Description,
                    Amount = parsedAmount, 
                    Type = resolvedType,
                    CategoryId = this.SelectedCategory!.Id,
                    TransactionCategory = this.SelectedCategory 
                };
                _dbContext.Transactions.Add(newTransaction);
            }
            else
            {
                // Obohacení té staré o upravená data
                _originalTransaction.Date = this.Date;
                _originalTransaction.Description = this.Description;
                _originalTransaction.Amount = parsedAmount; 
                _originalTransaction.Type = resolvedType;
                _originalTransaction.CategoryId = this.SelectedCategory!.Id;
                _originalTransaction.TransactionCategory = this.SelectedCategory;
            }

            _dbContext.SaveData();
            RequestClose?.Invoke(); // Povzbuzení zavření (odchod do TransactionListViewModel)
        }

        /// <summary>
        /// Zavolá požadavek na zrušení dialogu, což umožní uživateli opustit formulář bez uložení změn.
        /// </summary>
        private void ExecuteCancel()
        {
            RequestClose?.Invoke();
        }
    }
}
