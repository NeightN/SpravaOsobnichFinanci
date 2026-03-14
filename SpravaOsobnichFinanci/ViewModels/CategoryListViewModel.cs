using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Models;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Spravuje logiku zobrazení, mazání a iniciování editace v seznamu všech dostupných kategorií.
    /// Obstarává komunikaci mezi UI tabulkou (DataGrid/List) a databázovou entitou Categories.
    /// </summary>
    internal class CategoryListViewModel : BaseViewModel
    {
        private readonly DatabaseContext _dbContext;
        
        // Použití ObservableCollection zaručuje, že jakékoliv přidání/smazání záznamu se okamžitě projeví v rozhraní
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        // Příkazy pro ovládací prvky v UI, které spouští logiku pro přidání, editaci nebo smazání kategorie.
        // Implementace logiky je skryta v metodách ExecuteAddCategory, ExecuteEditCategory a ExecuteDeleteCategory.
        private readonly ICommand _addCategoryCommand;
        private readonly ICommand _editCategoryCommand;
        private readonly ICommand _deleteCategoryCommand;

        /// <summary>
        /// Kolekce všech kategorií načtených z databáze. 
        /// Jakékoliv změny v této kolekci (přidání, smazání) se automaticky projeví v UI díky ObservableCollection.
        /// </summary>
        public ObservableCollection<Category> Categories
        {
            get => _categories; 
            set => SetProperty(ref _categories, value); 
        }

        // --- Příkazy svázané s tlačítky v UI (Binding) ---
        public ICommand AddCategoryCommand => _addCategoryCommand;
        public ICommand EditCategoryCommand => _editCategoryCommand;
        public ICommand DeleteCategoryCommand => _deleteCategoryCommand; 

        /// <summary>
        /// Událost upozorňující nadřazený ViewModel, že má přepnout obrazovku na editor kategorií.
        /// Pokud dorazí parametr null, vytváří se nová. V opačném případě editujeme předanou.
        /// </summary>
        public event Action<Category?>? RequestEditCategory;

        /// <summary>
        /// Konstruktor, který přijímá databázový kontext pro načítání a manipulaci s daty kategorií.
        /// </summary>
        /// <param name="dbContext"></param>
        public CategoryListViewModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            
            // Propojení sledované kolekce ze současného stavu v databázi
            Categories = new ObservableCollection<Category>(_dbContext.Categories);

            _addCategoryCommand = new RelayCommand(ExecuteAddCategory);
            _editCategoryCommand = new RelayCommand<Category>(ExecuteEditCategory);
            _deleteCategoryCommand = new RelayCommand<Category>(ExecuteDeleteCategory); 
        }

        /// <summary>
        /// Spouští se při kliknutí na tlačítko "Přidat kategorii". 
        /// Vyvolá událost RequestEditCategory s parametrem null, což signalizuje otevření editoru pro vytvoření nové kategorie.
        /// </summary>
        private void ExecuteAddCategory()
        {
            RequestEditCategory?.Invoke(null);
        }

        /// <summary>
        /// Spouští se při kliknutí na tlačítko "Upravit" u konkrétní kategorie. 
        /// Vyvolá událost RequestEditCategory s předanou kategorií, což signalizuje otevření editoru pro úpravu této kategorie.
        /// </summary>
        /// <param name="categoryToEdit"> Kategorie předaná jako parametr z označení v tabulce.</param>
        private void ExecuteEditCategory(Category? categoryToEdit)
        {
            if (categoryToEdit != null)
            {
                RequestEditCategory?.Invoke(categoryToEdit);
            }
        }

        /// <summary>
        /// Pokusí se smazat zvolenou kategorii. Metoda před svým provedením kontroluje referenční integritu.
        /// </summary>
        /// <param name="categoryToDelete">Kategorie předaná jako parametr z označení v tabulce.</param>
        private void ExecuteDeleteCategory(Category? categoryToDelete)
        {
            if (categoryToDelete == null) return;

            // Kontrola aplikační logiky na závislosti.
            // Pokud by se kategorie s existujícími transakcemi smazala, napojené finanční záznamy
            // by ztratily kontext, což by vedlo k nekonzistenci (a chybě) v databázi.
            bool isUsed = _dbContext.Transactions.Any(t => t.CategoryId == categoryToDelete.Id);
            if (isUsed)
            {
                SpravaOsobnichFinanci.Views.CustomMessageBox.ShowWarning(
                    $"Kategorii '{categoryToDelete.Name}' nelze smazat, protože je přiřazena k jedné nebo více transakcím. Nejprve změňte kategorii u těchto transakcí.",
                    "Nelze smazat",
                    null);
                return;
            }

            // Ochrana proti neúmyslnému smazání se strany uživatele - CustomMessageBox nahrazující nativní ošklivá Windows okénka
            bool result = SpravaOsobnichFinanci.Views.CustomMessageBox.Show(
                $"Opravdu chcete smazat kategorii '{categoryToDelete.Name}'?",
                "Potvrzení smazání",
                null);

            if (result)
            {
                // Musíme smazat lokálně z paměti pro DB záchyt i z rozhraní (ObservableCollection) 
                _dbContext.Categories.Remove(categoryToDelete);
                Categories.Remove(categoryToDelete);
                
                // Trvalý zápis na disk do SQLite db
                _dbContext.SaveData();
            }
        }
    }
}
