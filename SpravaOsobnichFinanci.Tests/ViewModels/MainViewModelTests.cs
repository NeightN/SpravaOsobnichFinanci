using System;
using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using Xunit;
using Microsoft.EntityFrameworkCore; 

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Slouží pro ověření logiky routování/přepínání oken skrz nejvyšší ViewModel v architektuře.
    /// Na rozdíl od ostatních testů nevyužívá Dependency Injection pro mock databáze, ale generuje vnitřně ostrý soubor SQLite.
    /// Z toho důvodu korektně implementuje IDisposable k zajištění zničení dočasných souborů po doběhnutí třídy.
    /// </summary>
    public class MainViewModelTests : IDisposable
    {
        public MainViewModelTests()
        {
             // Vynucení založení reálného databázového souboru před začátkem testů 
             // Ochrana před chybou kolize souborů (File in Use) při paralelním běhu xUnit
             using var db = new DatabaseContext();
             db.Database.EnsureCreated();
        }

        public void Dispose()
        {
             // Oklízecí metoda Garbage Collectoru. Test třída za sebou fyzicky soubor s testovací DB na konci smaže.
             using var db = new DatabaseContext();
             db.Database.EnsureDeleted();
        }
        
        [Fact]
        public void Constructor_Initialization_SetsDashboardAsDefaultView()
        {
            // --- ARRANGE & ACT ---
            var mainViewModel = new MainViewModel();

            // --- ASSERT ---
            // Po startu aplikačního cyklu musí být ihned načtena úvodní obrazovka do proměnné svázané s DataTemplate
            Assert.NotNull(mainViewModel.CurrentViewModel);
            Assert.IsType<DashboardViewModel>(mainViewModel.CurrentViewModel);
            Assert.Equal("Dashboard", mainViewModel.CurrentViewName);
        }

        [Fact]
        public void NavigateToTransactionsCommand_ChangesCurrentViewModel()
        {
            // --- ARRANGE ---
            var mainViewModel = new MainViewModel();

            // --- ACT ---
            // Simulace kliknutí na tlačítko levého navigačního menu "Transakce"
            mainViewModel.NavigateToTransactionsCommand.Execute(null);

            // --- ASSERT ---
            // Verifikace bezchybného prokliku - instance pohledu se musí za běhu nahradit modelem Historie (Seznam) transakcí.
            Assert.IsType<TransactionListViewModel>(mainViewModel.CurrentViewModel);
            Assert.Equal("Transactions", mainViewModel.CurrentViewName);
        }

        [Fact]
        public void NavigateToSettingsCommand_ChangesCurrentViewModel()
        {
            // --- ARRANGE ---
            var mainViewModel = new MainViewModel();

            // --- ACT ---
            // Simulace spuštění změny okna na nastavení
            mainViewModel.NavigateToSettingsCommand.Execute(null);

            // --- ASSERT ---
            Assert.IsType<SettingsViewModel>(mainViewModel.CurrentViewModel);
            Assert.Equal("Settings", mainViewModel.CurrentViewName);
        }

        [Fact]
        public void NavigateToCategoriesCommand_ChangesCurrentViewModel()
        {
            // --- ARRANGE ---
            var mainViewModel = new MainViewModel();

            // --- ACT ---
            mainViewModel.NavigateToCategoriesCommand.Execute(null);

            // --- ASSERT ---
            Assert.IsType<CategoryListViewModel>(mainViewModel.CurrentViewModel);
            Assert.Equal("Categories", mainViewModel.CurrentViewName);
        }
    }
}
