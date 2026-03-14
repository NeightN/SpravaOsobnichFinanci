using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Models;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Řídí logiku dialogového okna pro vytváření nových a úpravu stávajících kategorií.
    /// Zapouzdřuje validaci vstupů a ukládání změn do databáze bez přímé vazby na samotný proků okna.
    /// </summary>
    internal class CategoryEditorViewModel : BaseViewModel
    {
        // Odkaz na databázový kontext pro načítání a ukládání dat. Je předáván z nadřazené vrstvy (např. z hlavního ViewModelu) a umožňuje přístup k metodám pro manipulaci s daty.
        private readonly DatabaseContext _dbContext;
        
        // Rozlišuje režim ViewModelu: Null = Vytváříme novou kategorii záznam. Not-null = Editujeme.
        private readonly Category? _originalCategory;

        // Interní pole pro uchování aktuálního stavu formuláře. Tyto hodnoty se mění při editaci, ale původní kategorie zůstává nedotčena, dokud uživatel neklikne "Uložit".
        private string _name = string.Empty;
        private string _colorHex = "#000000";
        private string _iconKey = "Default";

        // Fixní předpřipravené volby pro rozbalovací seznamy (ComboBox) v UI.
        private readonly ObservableCollection<string> _availableIconKeys;
        private readonly ObservableCollection<string> _availableColors;

        // Příkazy pro ovládací prvky v UI, které spouští logiku pro uložení nebo zrušení akce.
        private readonly ICommand _saveCommand;
        private readonly ICommand _cancelCommand;

        public string Name
        {
            get => _name; 
            set => SetProperty(ref _name, value); 
        }

        public string ColorHex
        {
            get => _colorHex; 
            set => SetProperty(ref _colorHex, value); 
        }

        public string IconKey
        {
            get => _iconKey; 
            set => SetProperty(ref _iconKey, value); 
        }

        // Exponované kolekce pro naplnění ComboBoxů v UI. Jsou read-only z pohledu View, protože se mění pouze interně v konstruktoru.
        public ObservableCollection<string> AvailableIconKeys => _availableIconKeys;
        public ObservableCollection<string> AvailableColors => _availableColors;

        // Exponované příkazy pro tlačítka "Uložit" a "Zrušit" v UI. Implementace logiky je skryta v metodách ExecuteSave a ExecuteCancel.
        public ICommand SaveCommand => _saveCommand;
        public ICommand CancelCommand => _cancelCommand;

        /// <summary>
        /// Pošle signál nadřazenému View (oknu), že proces skončil (nebo byl zrušen) a okno by se mělo zavřít.
        /// Chrání to striktní separaci – ViewModel přímo nevolá metody z knihovny System.Windows (Window.Close).
        /// </summary>
        public event Action? RequestClose;

        /// <summary>
        /// Konstruktor pro CategoryEditorViewModel. Přijímá databázový kontext pro načítání a ukládání dat a volitelnou kategorii pro editaci.
        /// </summary>
        /// <param name="dbContext"> Odkaz na databázový kontext pro přístup k datům</param>
        /// <param name="category"> Volitelná kategorie pro editaci. Pokud je null, ViewModel se připraví pro vytvoření nové kategorie.</param>
        public CategoryEditorViewModel(DatabaseContext dbContext, Category? category = null)
        {
            _dbContext = dbContext;
            _originalCategory = category;

            _availableIconKeys = new ObservableCollection<string>
            {
                "Default", "Food", "Home", "Car", "Entertainment", "Health", "Shopping", 
                "Money", "Bank", "Clothes", "Pets", "Education", "Travel", "Gifts", 
                "Coffee", "Phone", "Internet"
            };

            // Paleta hezkých barev ve stylu Google Material Design
            _availableColors = new ObservableCollection<string>
            {
                "#F44336", "#E91E63", "#9C27B0", "#3F51B5", "#2196F3", "#03A9F4", 
                "#00BCD4", "#009688", "#4CAF50", "#8BC34A", "#CDDC39", "#FFEB3B", 
                "#FFC107", "#FF9800", "#FF5722", "#795548", "#9E9E9E", "#607D8B"
            };

            _saveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            _cancelCommand = new RelayCommand(ExecuteCancel);

            // Nastavení výchozích hodnot polí حسب toho, zda přidáváme či editujeme.
            if (_originalCategory != null)
            {
                Name = _originalCategory.Name;
                ColorHex = _originalCategory.ColorHex;
                // Pojistka proti tomu, pokud by byla z DB načtena ikona, která už ve stávajícím seznamu neexistuje
                IconKey = AvailableIconKeys.Contains(_originalCategory.IconKey) ? _originalCategory.IconKey : "Default";
            }
            else
            {
                ColorHex = AvailableColors[4]; // Výchozí barva formuláře při založení nové kategorie (modrá)
            }
        }

        /// <summary>
        /// Validace pro povolení tlačítka "Uložit". Zajišťuje, že uživatel nemůže uložit kategorii bez vyplnění povinných polí (název a barva).
        /// </summary>
        /// <returns>True, pokud jsou všechna povinná pole vyplněna, jinak False.</returns>
        private bool CanExecuteSave()
        {
            // Tlačítko (Command) zakázáno, dokud uživatel nevyplní povinná pole.
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(ColorHex);
        }

        /// <summary>
        /// Logika pro uložení kategorie do databáze. Pokud se jedná o novou kategorii, vytvoří se nový záznam. Pokud se jedná o editaci, aktualizují se vlastnosti stávající kategorie.
        /// </summary>
        private void ExecuteSave()
        {
            if (_originalCategory == null)
            {
                // Režim "Nový záznam"
                var newCategory = new Category
                {
                    Name = this.Name,
                    ColorHex = this.ColorHex,
                    IconKey = this.IconKey
                };
                _dbContext.Categories.Add(newCategory);
            }
            else
            {
                // Režim "Editace" - entity framework si objekt stávající kategorie drží v paměti (tzv. tracking),
                // takže stačí jen změnit mu vnitřní vlastnosti a on je při SaveData() přeuloží do DB.
                _originalCategory.Name = this.Name;
                _originalCategory.ColorHex = this.ColorHex;
                _originalCategory.IconKey = this.IconKey;
            }

            _dbContext.SaveData();
            RequestClose?.Invoke();
        }

        /// <summary>
        /// Logika pro zrušení akce. Neprovádí žádné změny v databázi, pouze zavolá RequestClose, aby se okno zavřelo a všechny neuložené změny v in-memory polích zanikly.
        /// </summary>
        private void ExecuteCancel()
        {
            // Přeruší akci bez volání uložení do databáze (změny v in-memory fields jednoduše zaniknou se zavřením okna).
            RequestClose?.Invoke();
        }
    }
}