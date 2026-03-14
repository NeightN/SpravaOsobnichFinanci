using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Ovìøuje funkènost naèítání a správy listu s Kategorie. 
    /// Pro eliminaci zásahu do produkèní databáze a zpomalování sítì/disku využívá EF Core In-Memory izolovanou instanci.
    /// </summary>
    public class CategoryListViewModelTests
    {
        /// <summary>
        /// Sestaví v pamìti operující SQLite kontext (In-Memory). Dovoluje tvoøit izolaèní prostøedí plné "falešných" simulativních dat.
        /// </summary>
        private DatabaseContext CreateTestDatabase()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                // Vynucení In-Memory režimu sdílejícím specifické unikátní jméno (ZABRÁNÍ TESTÙM V PØEPSÁNÍ NAJEDNOU)
                .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
                .Options;

            var db = new DatabaseContext(options);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();

            // Vytvoøíme cvièný záznam pro úèely testu
            db.Categories.Add(new Category { Name = "Testovací Kategorie", ColorHex = "#FF0000", IconKey = "Food" });
            db.SaveChanges(); 

            return db;
        }

        [Fact]
        public void Constructor_Initialization_LoadsCategoriesFromDatabase()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase(); // V mockDb máme 1 kategorii viz výše

            // --- ACT ---
            var viewModel = new CategoryListViewModel(mockDb);

            // --- ASSERT ---
            Assert.NotNull(viewModel.Categories);
            
            // Standardní Data Seeding z DatabaseContextu vsazuje 16 systémových kategorií. 
            // My pøidali jednu dodateènou v metodì výše. Oèekáváme tedy 17 výskytù.
            Assert.Equal(17, viewModel.Categories.Count); 
            
            // Provìrka, zda se model nepoškodil a texty do kolekce prostoupily nedeformované.
            Assert.Contains(viewModel.Categories, c => c.Name == "Testovací Kategorie");
        }

        [Fact]
        public void ExecuteAddCategoryCommand_InvokesRequestEditCategoryEvent()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase();
            var viewModel = new CategoryListViewModel(mockDb);

            bool eventFired = false;
            Category? invokedCategory = new Category(); 

            // Subskripce stejná, jako na pozadí volá navigaèní Flow v Root (Main) ViewModelu
            viewModel.RequestEditCategory += (categoryArg) =>
            {
                eventFired = true;
                invokedCategory = categoryArg;
            };

            // --- ACT ---
            viewModel.AddCategoryCommand.Execute(null);

            // --- ASSERT ---
            // Pøi kliknutí na tlaèítko "Nová kategorie" oèekáváme, že se událost bezpeènì vyvolá
            // a pøepošle se skrze ni do editoru hodnota null (Znaèící režim 'Pøidání').
            Assert.True(eventFired);
            Assert.Null(invokedCategory);
        }

        [Fact]
        public void ExecuteEditCategoryCommand_WithCategory_InvokesRequestEditCategoryEvent()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase();
            var viewModel = new CategoryListViewModel(mockDb);

            var categoryToEdit = mockDb.Categories.First(); // Vezmeme tu naši 1 cviènou kategorii

            bool eventFired = false;
            Category? invokedCategory = null;

            viewModel.RequestEditCategory += (categoryArg) =>
            {
                eventFired = true;
                invokedCategory = categoryArg;
            };

            // --- ACT ---
            // Skrze RelayCommand pošleme parametr reprezentující vybraný záznam z UI DataGrid tabulky
            viewModel.EditCategoryCommand.Execute(categoryToEdit);

            // --- ASSERT ---
            Assert.True(eventFired);
            
            // Okno editoru se díky zaslané události spustí nikoliv v režimu Vytváøení, ale Úprav,  
            // protože mu ViewModel posílá k identifikaci pøesnì tento starý model.
            Assert.Equal(categoryToEdit, invokedCategory); 
        }
    }
}