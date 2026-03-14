using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Zajišťuje absolutní odolnost platebního editoru proti nekorektním uživatelským vstupům,
    /// ať už jde o prázdné platby nebo typografické omyly při vkládání plovoucí čárky u částek.
    /// </summary>
    public class TransactionEditorViewModelTests
    {
        private DatabaseContext CreateTestDatabase()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
                .Options;

            var db = new DatabaseContext(options);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();
            
            db.Categories.Add(new Category { Name = "Základní kategorie", Id = Guid.NewGuid(), ColorHex="#000", IconKey="Food" });
            db.SaveChanges();
            
            return db;
        }

        // xUnit využití [Theory] místo standardního [Fact]. 
        // Tento přístup injektuje hodnoty InlineData do stejného těla testu a zabraňuje repetitivnímu duplikování testovacího kódu.
        [Theory] 
        [InlineData("125.50")] // Testovaný scénář se syntaktickou anglickou tečkou
        [InlineData("125,50")] // Testovaný scénář se standardní českou čárkou
        public void AmountInput_ReplacesDotWithComma(string inputValue)
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase();
            var viewModel = new TransactionEditorViewModel(mockDb, null);

            // --- ACT ---
            viewModel.AmountInput = inputValue;

            // --- ASSERT ---
            // Backendové vlastnosti MVVM navázaného formuláře by měly zafungovat jako autokorekce a překonat 
            // mezinárodní kulturní konflikty zahozením teček ve prospěch bezpečně parsovatelné čárky české kultury.
            Assert.Equal("125,50", viewModel.AmountInput);
            Assert.Contains(",", viewModel.AmountInput);
            Assert.DoesNotContain(".", viewModel.AmountInput);
        }

        [Fact]
        public void SaveCommand_CannotExecute_WhenAmountIsZeroOrInvalid()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase();
            var viewModel = new TransactionEditorViewModel(mockDb, null);

            viewModel.SelectedCategory = mockDb.Categories.First();

            // --- ACT & ASSERT ---
            
            // Fáze 1: Nulová částka. Validní struktura znaků i kategorie, ale logicky neexistující byznysová hodnota pro platbu.
            viewModel.AmountInput = "0";
            Assert.False(viewModel.SaveCommand.CanExecute(null));

            // Fáze 2: Nečíselné / Alfabetické vstupy do TextBoxu. Převedení uvnitř ViewModelu musí selhat (TryParse) a zablokovat ukládání.
            viewModel.AmountInput = "abc";
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void SaveCommand_CanExecute_WhenMinimumRequiredInputsAreValid()
        {
            // --- ARRANGE ---
            var mockDb = CreateTestDatabase();
            var viewModel = new TransactionEditorViewModel(mockDb, null);

            // --- ACT ---
            // Účelová simulace absolutně nejchudšího možného vstupu uživatele - vynechání popisku platby.
            viewModel.Description = ""; 
            viewModel.AmountInput = "250";
            viewModel.SelectedCategory = mockDb.Categories.First();

            // --- ASSERT ---
            // Uložitelnost ViewModelu se nesmí opírat o přítomnost popisku. Transakci musíme připustit k uložení.
            Assert.True(viewModel.SaveCommand.CanExecute(null));
        }
    }
}
