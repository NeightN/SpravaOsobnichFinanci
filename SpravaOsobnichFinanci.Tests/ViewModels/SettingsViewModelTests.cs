using System;
using Microsoft.EntityFrameworkCore;
using SpravaOsobnichFinanci.Converters;
using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Ověřuje spolehlivost aplikačního nastavení a schopnost uživatele modifikovat globální symboly (změna měny).
    /// </summary>
    public class SettingsViewModelTests : BaseViewModelTests
    {
        /// <summary>
        /// Rychlé generování prázdné In-Memory struktury db pro obsluhu sekce Settings.
        /// </summary>
        private DatabaseContext GetTestDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
                .Options;

            var db = new DatabaseContext(options);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();

            // Sestrojení výchozího stavu k nastavení testovacích předpokladů (Baseline)
            db.Settings.CurrencySymbol = "Kč";
            db.SaveChanges();
            
            return db;
        }

        [Fact]
        public void Constructor_Initialization_LoadsAvailableCurrenciesAndSelectedCurrency()
        {
            // --- ARRANGE ---
            var mockDb = GetTestDatabaseContext();
            
            // Simulační injektáž stavu, ke které na pozadí aplikace rutinně dochází z bootu MainViewModelu
            CurrencyConverter.CurrentSymbol = mockDb.Settings.CurrencySymbol; 

            // --- ACT ---
            var viewModel = new SettingsViewModel(mockDb);

            // --- ASSERT ---
            // ViewModel by si sám měl ve svém Ctor sestavit ObservableCollection 4 pevných měn určených pro ComboBox
            Assert.NotNull(viewModel.AvailableCurrencies);
            Assert.Equal(4, viewModel.AvailableCurrencies.Count);
            
            // A aktuálně vybranou měnu musí mít nastavenou z uložené informace databáze
            Assert.Equal("Kč", viewModel.SelectedCurrency);
        }

        [Fact]
        public void ExecuteSaveSettingsCommand_UpdatesGlobalCurrencyAndDatabaseSettings()
        {
            // --- ARRANGE ---
            var mockDb = GetTestDatabaseContext();
            var viewModel = new SettingsViewModel(mockDb);

            // Substituce výběru z ComboBoxu pomocí uživatelského rozhraní
            viewModel.SelectedCurrency = "€";

            // --- ACT ---
            viewModel.SaveSettingsCommand.Execute(null);

            // --- ASSERT ---
            // Primární ověření: hodnota musí dosáhnout vrstvy trvalého úložiště
            Assert.Equal("€", mockDb.Settings.CurrencySymbol);
            
            // Sekundární (zásadnější!) ověření: hodnota se musí ihned promítnout do celosystémového konvertoru,
            // což garantuje okamžité překreslení cifer napříč kompletním aplikačním rozhraním.
            Assert.Equal("€", CurrencyConverter.CurrentSymbol);
        }
    }
}
