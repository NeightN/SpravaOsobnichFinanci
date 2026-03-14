using SpravaOsobnichFinanci.Commands;
using System;
using Xunit;

namespace SpravaOsobnichFinanci.Tests.Commands
{
    /// <summary>
    /// Ověřuje správnou funkčnost základního stavebního kamene MVVM - RelayCommandu.
    /// Využívá xUnit framework a striktně dodržuje strukturu AAA (Arrange, Act, Assert).
    /// </summary>
    public class RelayCommandTests
    {
        [Fact]
        public void Execute_InvokesActionSuccessfully()
        {
            // --- ARRANGE (Příprava prostředí) ---
            bool wasMethodCalled = false; // Detekční příznak, který se změní jen uvnitř delegáta
            
            // Zainciování příkazu pomocí lambda (anonymí metody
            var command = new RelayCommand(() => { wasMethodCalled = true; });

            // --- ACT (Vykonání logiky) ---
            command.Execute(null);

            // --- ASSERT (Ověření výsledku) ---
            // Zjišťuje, zda Execute interně opravdu zavolal námi podvrhnutou lambda metodu
            Assert.True(wasMethodCalled);
        }

        [Fact]
        public void CanExecute_NoPredicate_ReturnsTrue()
        {
            // --- ARRANGE (Příprava prostředí) ---
            // Iniciujeme příkaz BEZ omezující podmínky CanExecute (tlačítka v UI by tak měla být vždy aktivní)
            var command = new RelayCommand(() => { });

            // --- ACT (Vykonání logiky) ---
            var result = command.CanExecute(null);

            // --- ASSERT (Ověření výsledku) ---
            // Očekáváme, že se příkaz sám vyhodnotí jako povolený
            Assert.True(result);
        }

        [Fact]
        public void Constructor_NullAction_ThrowsArgumentNullException()
        {
            // U testování Exception (výjimek) se fáze Act i Assert provádějí souběžně přes Assert.Throws

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Zákaz varování CS8625 je pro účely tohoto testu nutný a schválený, logice se snažíme úmyslně 
                // podvrhnout null a nesmíme dovolit kompilátoru (který to odhalí dříve), aby test vyřadil jako nesestavitelný.
#pragma warning disable CS8625
                var command = new RelayCommand(null);
#pragma warning restore CS8625
            });
        }
    }
}
