using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore; 

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Ovìøuje, zda hlavní vyhledávací vrstva aplikace (ICollectionView List a TextBoxy) správnì reaguje a maskuje záznamy dle kritérií.
    /// </summary>
    public class TransactionListViewModelTests
    {
        private DatabaseContext CreateTestDatabaseWithTransactions()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
                .Options;

            var db = new DatabaseContext(options);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();

            var foodCategory = new Category { Name = "Jídlo", Id = Guid.NewGuid() };
            var salaryCategory = new Category { Name = "Výplata", Id = Guid.NewGuid() };
            
            db.Categories.Add(foodCategory);
            db.Categories.Add(salaryCategory);

            // Sestrojení diverzifikované testovací palety transakcí, 1 výdaj, 1 pøíjem.
            db.Transactions.Add(new Transaction 
            { 
                Description = "Nákup Albert", 
                Amount = 1500, 
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                CategoryId = foodCategory.Id,
                TransactionCategory = foodCategory
            });

            db.Transactions.Add(new Transaction 
            { 
                Description = "Výplata za bøezen", 
                Amount = 45000, 
                Type = TransactionType.Income,
                Date = DateTime.Now,
                CategoryId = salaryCategory.Id,
                TransactionCategory = salaryCategory
            });
            
            db.SaveChanges();
            return db;
        }

        [Fact]
        public void InitialFilterState_ShowsAllCurrentMonthTransactions()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseWithTransactions();
            
            // --- ACT ---
            var viewModel = new TransactionListViewModel(mockDb);

            // --- ASSERT ---
            // Po otevøení okna (Listu transakcí) by pøed jakoukoliv manipulací by uživatelem mìl list obsahovat vše propuštìné skrze ICollectionView.
            var visibleItemsCount = viewModel.TransactionsView?.Cast<Transaction>().Count();
            Assert.Equal(2, visibleItemsCount);
        }

        [Fact]
        public void SearchTextFilter_WhenSet_FiltersTransactionsCorrectly()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseWithTransactions();
            var viewModel = new TransactionListViewModel(mockDb);

            // --- ACT ---
            // Tenzorové vyhledávání uživatele na pøesnou shodu textového øetìzce
            viewModel.SearchText = "Albert";

            // --- ASSERT ---
            var visibleTransactions = viewModel.TransactionsView?.Cast<Transaction>().ToList();
            
            // Verifikace správnosti "oøezání" nadbyteèných dat. List musí obsahovat jediný záznam a to specificky nákup.
            Assert.NotNull(visibleTransactions);
            Assert.Single(visibleTransactions);
            Assert.Equal("Nákup Albert", visibleTransactions.First().Description); 
        }

        [Fact]
        public void SelectedFilterType_SetToIncome_ShowsOnlyIncomes()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseWithTransactions();
            var viewModel = new TransactionListViewModel(mockDb);

            // --- ACT ---
            // Pøepnutí ComboBoxu na filtr pøíjmù
            viewModel.SelectedFilterType = "Pøíjem"; 

            // --- ASSERT ---
            var visibleTransactions = viewModel.TransactionsView?.Cast<Transaction>().ToList();

            Assert.NotNull(visibleTransactions);
            Assert.Single(visibleTransactions);
            Assert.Equal("Výplata za bøezen", visibleTransactions.First().Description); 
            Assert.Equal(TransactionType.Income, visibleTransactions.First().Type);
        }

        [Fact]
        public void ExecuteClearFilterCommand_ResetsAllFiltersAndShowsAll()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabaseWithTransactions();
            var viewModel = new TransactionListViewModel(mockDb);

            // Napodobení stavu, kdy uživatel neplatnì a zmatenì pøekombinoval vyhledávací dotazy a nic mu to nenašlo.
            viewModel.SearchText = "TotoTuNení";
            viewModel.SelectedFilterType = "Výdaj";
            
            var visibleItemsCountBefore = viewModel.TransactionsView?.Cast<Transaction>().Count();
            Assert.Equal(0, visibleItemsCountBefore);

            // --- ACT ---
            // Interakce smazat/znovu naèíst okno listu
            viewModel.ClearFilterCommand.Execute(null);

            // --- ASSERT ---
            // Ovìøení návratu ViewModelových datových prvkù (Properties) do defaultních pre-boot pozic, a následné vrácení vizibility obsahu.
            Assert.Equal(string.Empty, viewModel.SearchText);
            Assert.Equal("Vše", viewModel.SelectedFilterType);
            
            var visibleItemsCountAfter = viewModel.TransactionsView?.Cast<Transaction>().Count();
            Assert.Equal(2, visibleItemsCountAfter);
        }
    }
}