using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using Microsoft.EntityFrameworkCore; 
using System;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Pokrývá byznysovou logiku Dashboardu – tedy součtové matematické agregace pro vykreslování v UI a přepínání časových oken.
    /// K testování matematických operací využívá striktně připravená Dummy data zasílaná do In-Memory databázového poskytovatele.
    /// </summary>
    public class DashboardViewModelTests
    {
        /// <summary>
        /// Vyčlení v operační paměti databázový kontext simulující SQLite databázi obsahující přesná, kontrolovatelná data.
        /// Tvoří jeden starý historický záznam a dva novodobé, na čemž testujeme správnost agregací v čase.
        /// </summary>
        private DatabaseContext CreateTestDatabaseForStatistics()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
                .Options;

            var db = new DatabaseContext(options);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();

            // Sestrojení validní Fake kategorie obcházející cizí klíče
            var cat = new Category { Name = "Test", ColorHex = "#FF0000", IconKey="Food" };
            db.Categories.Add(cat);

            // Transakce -1 měsíc nazpět
            db.Transactions.Add(new Transaction 
            { 
                Amount = 10000, Type = TransactionType.Income, Date = DateTime.Now.AddMonths(-1), 
                TransactionCategory = cat, CategoryId = cat.Id 
            });

            // Transakce ode dneška (Current Month)
            db.Transactions.Add(new Transaction 
            { 
                Amount = 30000, Type = TransactionType.Income, Date = DateTime.Now, 
                TransactionCategory = cat, CategoryId = cat.Id 
            });

            db.Transactions.Add(new Transaction 
            { 
                Amount = 12000, Type = TransactionType.Expense, Date = DateTime.Now, 
                TransactionCategory = cat, CategoryId = cat.Id
            });
            
            db.SaveChanges();

            return db;
        }

        [Fact]
        public void CalculateSummaries_CalculatesTotalBalanceCorrectly_ForWholeHistory()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseForStatistics();
            
            // --- ACT ---
            // Sám konstruktor ViewModelu zavolá metodu RefreshDashboard a sestaví sumáře
            var viewModel = new DashboardViewModel(mockDb); 

            // --- ASSERT ---
            // Očekáváme celkový čistý součet přes celou historii:
            // 10 000 (Minulý příjem) + 30 000 (Aktuální příjem) - 12 000 (Aktuální Výdaj) = 28 000 
            Assert.Equal(28000m, viewModel.TotalBalance);
        }

        [Fact]
        public void CalculateSummaries_CalculatesMonthlyStatisticsCorrectly_ForCurrentMonth()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseForStatistics();

            // --- ACT ---
            var viewModel = new DashboardViewModel(mockDb); 

            // --- ASSERT ---
            // Nyní ověřujeme rozštěpení pouze na daný aktuální měsíc (Karta Dashboard: Aktuální měsíc). 
            // Historických 10 000 tu chybí.
            Assert.Equal(30000m, viewModel.MonthlyIncome);
            Assert.Equal(12000m, viewModel.MonthlyExpense);
            Assert.Equal(18000m, viewModel.MonthlyBalance);
        }

        [Fact]
        public void PreviousMonthCommand_UpdatesSelectedMonthAndRefreshesData()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseForStatistics();
            var viewModel = new DashboardViewModel(mockDb); 
            DateTime expectedMonth = DateTime.Now.AddMonths(-1);

            // --- ACT ---
            // Spuštění RelayCommandu svázaného běžně s levým tlačítkem šipky
            viewModel.PreviousMonthCommand.Execute(null);

            // --- ASSERT ---
            Assert.Equal(expectedMonth.Month, viewModel.SelectedMonth.Month);
            Assert.Equal(expectedMonth.Year, viewModel.SelectedMonth.Year);

            // Po simulovaném přepnutí do minulého měsíce zjistíme, zda se automaticky přepočítala data pro Kartu UI:
            // Očekává se starý historický příjem 10 000, ale výdaj 0, jelikož žádný tehdy nebyl proveden
            Assert.Equal(10000m, viewModel.MonthlyIncome); 
            Assert.Equal(0m, viewModel.MonthlyExpense); 
        }
    }
}
