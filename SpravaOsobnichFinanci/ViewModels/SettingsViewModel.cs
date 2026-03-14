using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SpravaOsobnichFinanci.Commands;
using SpravaOsobnichFinanci.Converters;
using SpravaOsobnichFinanci.Models;
using SpravaOsobnichFinanci.Views; 

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Zajišťuje logiku obrazovky nastavení v aplikaci. 
    /// Soustředí se na změnu globální měny, export uživatelských dat do souborů (CSV, JSON) 
    /// a možnost radikálního vymazání všech uživatelských dat (Factory Reset).
    /// </summary>
    internal class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseContext _dbContext;
        private string _selectedCurrency = "CZK";
        
        // Pevně definovaný list měn, ze kterých si uživatel může přes ComboBox vybrat.
        private readonly ObservableCollection<string> _availableCurrencies;

        // Příkazy pro interakci s tlačítky v grafickém rozhraní
        private readonly ICommand _saveSettingsCommand;
        private readonly ICommand _exportDataCommand;
        private readonly ICommand _deleteAllDataCommand;

        public string SelectedCurrency
        {
            get => _selectedCurrency; 
            set => SetProperty(ref _selectedCurrency, value); 
        }

        public ObservableCollection<string> AvailableCurrencies => _availableCurrencies;

        // --- Příkazy (Commands) pro tlačítka v grafickém rozhraní ---
        public ICommand SaveSettingsCommand => _saveSettingsCommand;
        public ICommand ExportDataCommand => _exportDataCommand; 
        public ICommand DeleteAllDataCommand => _deleteAllDataCommand;

        /// <summary>
        /// Konstruktor přijímá instanci DatabaseContextu pro přístup k nastavením a datům.
        /// </summary>
        /// <param name="dbContext">Instance třídy DatabaseContext pro práci s daty a nastaveními.</param>
        public SettingsViewModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;

            _availableCurrencies = new ObservableCollection<string>
            {
                "Kč", "€", "$", "£"
            };

            // Načtení stávající vybrané měny ze Singleton konvertoru při spuštění View
            _selectedCurrency = CurrencyConverter.CurrentSymbol;

            _saveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
            _exportDataCommand = new RelayCommand<string>(ExecuteExportData); 
            _deleteAllDataCommand = new RelayCommand(ExecuteDeleteAllData);
        }

        /// <summary>
        /// Uloží aktuálně vybranou měnu do statické třídy CurrencyConverter a zároveň aktualizuje trvalou hodnotu v databázi.
        /// </summary>
        private void ExecuteSaveSettings()
        {
            // Aplikace změny do statické třídy i uložení trvalé hodnoty do DB
            CurrencyConverter.CurrentSymbol = SelectedCurrency;
            _dbContext.Settings.CurrencySymbol = SelectedCurrency;
            _dbContext.SaveData();

            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                CustomMessageBox.ShowWarning("Nastavení bylo úspěšně uloženo. Změna měny se projeví po přepnutí pohledu.", "Informace", Application.Current.MainWindow);
            }
        }
        
        /// <summary>
        /// Vyvolá nativní dialogové okno operačního systému (SaveFileDialog) pro výběr cesty uložení 
        /// a následně nasměruje flow buď do CSV nebo JSON exportovací logiky.
        /// </summary>
        private void ExecuteExportData(string? format)
        {
            if (!_dbContext.Transactions.Any())
            {
                CustomMessageBox.ShowWarning("Nejsou k dispozici žádná data k exportu.", "Upozornění", Application.Current.MainWindow);
                return;
            }

            if (string.IsNullOrEmpty(format)) return;

            // Příprava omezovacích filtrů pro Windows Průzkumníka souborů (uložit jako typ .csv / .json)
            string filter = format.ToUpper() == "CSV" ? "CSV soubor (*.csv)|*.csv" : "JSON soubor (*.json)|*.json";
            string defaultExt = format.ToUpper() == "CSV" ? ".csv" : ".json";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = $"Exportovat transakce do {format.ToUpper()}",
                Filter = filter,
                FileName = $"Export_Financi_{DateTime.Now:yyyyMMdd}",
                DefaultExt = defaultExt
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Volání konkrétní exportní metody v závislosti na parametru tlačítka (CommandParameter v XAMLu)
                    if (format.ToUpper() == "CSV") ExportCsvFile(saveFileDialog.FileName);
                    else if (format.ToUpper() == "JSON") ExportJsonFile(saveFileDialog.FileName);

                    CustomMessageBox.ShowWarning($"Data byla úspěšně exportována.", "Export dokončen", Application.Current.MainWindow);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.ShowWarning($"Při exportu dat došlo k chybě:\n{ex.Message}", "Chyba", Application.Current.MainWindow);
                }
            }
        }

        /// <summary>
        /// Sestaví obsah CSV souboru jako řetězec, přičemž každý záznam je oddělen středníkem a každý řádek reprezentuje jednu transakci.
        /// </summary>
        /// <param name="filePath">Cesta k souboru, kam bude CSV obsah uložen.</param>
        private void ExportCsvFile(string filePath)
        {
            var csvContent = new StringBuilder();
            
            // Hlavička CSV souboru pro tabulkové procesory jako MS Excel
            csvContent.AppendLine("Datum;Popis;Typ;Částka;Kategorie");

            foreach (var transaction in _dbContext.Transactions)
            {
                string categoryName = transaction.TransactionCategory != null ? transaction.TransactionCategory.Name : "Neznámá";
                string typeText = transaction.Type == TransactionType.Income ? "Příjem" : "Výdaj";
                
                // Převod každého záznamu na oddělený řetězec 
                csvContent.AppendLine($"{transaction.Date:dd.MM.yyyy};{transaction.Description};{typeText};{transaction.Amount};{categoryName}");
            }
            
            // Fyzický zápis sestaveného stringu na pevný disk 
            File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Vytvoří strukturovaný JSON řetězec z kolekce transakcí, přičemž každý záznam je reprezentován jako objekt s českými klíči.
        /// </summary>
        /// <param name="filePath">Cesta k souboru, kam bude JSON obsah uložen.</param>
        private void ExportJsonFile(string filePath)
        {
            // Abstrahování (projekce) reálného databázového modelu do anonymního typu 
            // vhodnějšího pro textové zobrazení v JSONu a s českými názvy klíčů.
            var exportData = _dbContext.Transactions.Select(t => new
            {
                Datum = t.Date.ToString("dd.MM.yyyy"),
                Popis = t.Description,
                Typ = t.Type == TransactionType.Income ? "Příjem" : "Výdaj",
                Castka = t.Amount,
                Kategorie = t.TransactionCategory != null ? t.TransactionCategory.Name : "Neznámá"
            }).ToList();

            // Nastavení Serializeru tak, aby výsledný JSON nespustil vše do jednoho řádku (WriteIndented),
            // a naopak správně uchoval českou interpunkci bez převodu do Unicode formátu (UnsafeRelaxedJsonEscaping).
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };
            
            string jsonString = JsonSerializer.Serialize(exportData, options);
            File.WriteAllText(filePath, jsonString, Encoding.UTF8);
        }

        /// <summary>
        /// Zobrazí potvrzovací dialog pro uživatele, aby se ujistil, že opravdu chce smazat všechna data.
        /// </summary>
        private void ExecuteDeleteAllData()
        {
            bool result = CustomMessageBox.Show(
                "Opravdu chcete smazat všechny transakce a kompletně obnovit výchozí kategorie i nastavení uživatele?\n\nTento krok je nevratný!", 
                "Varování před smazáním", 
                Application.Current.MainWindow);

            if (result)
            {
                _dbContext.ResetFactoryDefaults();
                
                // Synchronizace měny zpět na výchozí po resetu
                CurrencyConverter.CurrentSymbol = _dbContext.Settings.CurrencySymbol;
                SelectedCurrency = _dbContext.Settings.CurrencySymbol;
                
                // Nová, srozumitelná hláška o úspěchu
                CustomMessageBox.ShowWarning(
                    "Aplikace byla úspěšně obnovena do továrního nastavení. Všechna uživatelská data byla smazána a kategorie byly vráceny do výchozího stavu.", 
                    "Obnova dokončena", 
                    Application.Current.MainWindow);
            }
        }
    }
}