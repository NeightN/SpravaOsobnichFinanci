using SpravaOsobnichFinanci.ViewModels;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.ViewModels
{
    /// <summary>
    /// Ovìøuje správnou funkènost abstrakèní logiky BaseViewModelu.
    /// Soustøedí se primárnì na to, zda se do uživatelského rozhraní správnì odesílají
    /// PropertyChanged události po zmìnì dat.
    /// </summary>
    public class BaseViewModelTests
    {
        /// <summary>
        /// Úèelový 'Mock' objekt odvozený z BaseViewModelu. Je nadefinován jako privátní, 
        /// zanoøená tøída, èímž pøedcházíme kompilátorovým problémùm s nekonzistentní dostupností (CS0060).
        /// </summary>
        private class MockViewModel : BaseViewModel
        {
            private string _testProperty = string.Empty;

            public string TestProperty
            {
                get => _testProperty;
                set => SetProperty(ref _testProperty, value);
            }
        }

        [Fact]
        public void SetProperty_WhenValueChanges_RaisesPropertyChangedEvent()
        {
            // --- ARRANGE ---
            var viewModel = new MockViewModel();
            string changedPropertyName = string.Empty;

            // Nabindujeme se na událost (stejnì jako to dìlá WPF okno na pozadí)
            viewModel.PropertyChanged += (sender, args) =>
            {
                changedPropertyName = args.PropertyName ?? string.Empty;
            };

            // --- ACT ---
            // Zmìníme hodnotu vystavené vlastnosti
            viewModel.TestProperty = "Nová hodnota";

            // --- ASSERT ---
            // Oèekáváme, že z notifikace správnì pøijde jméno 'TestProperty' a hodnota na instanci bude nová.
            Assert.Equal(nameof(MockViewModel.TestProperty), changedPropertyName);
            Assert.Equal("Nová hodnota", viewModel.TestProperty);
        }

        [Fact]
        public void SetProperty_WhenValueStaysSame_DoesNotRaisePropertyChangedEvent()
        {
            // --- ARRANGE ---
            var viewModel = new MockViewModel();
            viewModel.TestProperty = "Stejná hodnota"; 
            
            bool eventRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                eventRaised = true; 
            };

            // --- ACT ---
            // Pøidejeme úplnì ten samý String
            viewModel.TestProperty = "Stejná hodnota"; 

            // --- ASSERT ---
            // SetProperty by to mìla detekovat jako shodu (EqualityComparer) a pøedèasit ukonèení (Return False).
            // UI by se tedy pøekreslovat nemìlo.
            Assert.False(eventRaised, "Událost PropertyChanged by se nemìla vyvolat, pokud se posílaná hodnota shoduje se stavem v pamìti.");
        }
    }
}