using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SpravaOsobnichFinanci.Models
{
    /// <summary>
    /// Hlavní přístupový bod k databázi pomocí EF Core.
    /// Zajišťuje mapování modelů na SQLite databázi.
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        private ApplicationSettings _settings = null!;
        private DbSet<Transaction> _transactions = null!;
        private DbSet<Category> _categories = null!;
        private DbSet<ApplicationSettings> _appSettings = null!;

        // === Databázové tabulky ===

        // Tabulka pro transakce, každá položka reprezentuje jeden záznam o příjmu nebo výdaji.
        public DbSet<Transaction> Transactions 
        { 
            get => _transactions; 
            set => _transactions = value; 
        }

        // Tabulka pro kategorie, které uživatel může přiřazovat k transakcím pro lepší organizaci.
        public DbSet<Category> Categories 
        { 
            get => _categories; 
            set => _categories = value; 
        }
        
        // Tabulka pro nastavení (očekává se vždy pouze jeden záznam)
        public DbSet<ApplicationSettings> AppSettings 
        { 
            get => _appSettings; 
            set => _appSettings = value; 
        }

        /// <summary>
        /// Aktuální nastavení aplikace načtené v paměti pro rychlý přístup z UI vrstvy.
        /// </summary>
        public ApplicationSettings Settings 
        { 
            get => _settings; 
            private set => _settings = value; 
        }

        /// <summary>
        /// Výchozí konstruktor, který zajišťuje vytvoření databáze (pokud neexistuje) a načtení dat do paměti.
        /// </summary>
        public DatabaseContext()
        {
            Database.EnsureCreated();
            LoadData();
        }

        /// <summary>
        /// Konstruktor přijímající nastavení (DbContextOptions). 
        /// Využívá se především pro Unit testy k podvrhnutí In-Memory databáze.
        /// </summary>
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
            LoadData();
        }

        /// <summary>
        /// Konfiguruje připojení k databázi. Metodu volá samotný EF Core.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Kontrola IsConfigured je nutná pro Unit testy. 
            // Pokud aplikaci nespouští test (který přes konstruktor podvrhuje in-memory databázi), 
            // nastavíme jako výchozí úložiště reálný lokální SQLite soubor.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=database.db");
            }
        }

        /// <summary>
        /// Uloží veškeré sledované změny do databáze.
        /// </summary>
        public void SaveData()
        {
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při ukládání dat do databáze: {ex.Message}");
            }
        }

        /// <summary>
        /// Načte data z databáze do paměti a případně provede úvodní inicializaci.
        /// </summary>
        public void LoadData()
        {
            try
            {
                // Inicializace nastavení - pokud záznam neexistuje, vytvoří se výchozí
                var settings = AppSettings.FirstOrDefault();
                if (settings == null)
                {
                    settings = new ApplicationSettings();
                    AppSettings.Add(settings);
                    SaveChanges();
                }
                Settings = settings;

                // Načtení kategorií nebo jejich prvotní naplnění
                if (!Categories.Any())
                {
                    CreateDefaultCategories();
                }
                else
                {
                    Categories.Load(); // Udržení entit v paměti pro lokální dotazy
                }

                // Načtení všech transakcí rovnou i s propojenou kategorií
                Transactions.Include(t => t.TransactionCategory).Load();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba při inicializaci tabulek: {ex.Message}");
            }
        }

        /// <summary>
        /// Naplní databázi výchozí sadou kategorií.
        /// Volá se pouze při prvním spuštění aplikace nebo po resetu 
        /// do továrního nastavení pro vytvoření základu.
        /// </summary>
        private void CreateDefaultCategories()
        {
            Categories.Add(new Category { Name = "Výplata", ColorHex = "#8BC34A", IconKey = "Money" });
            Categories.Add(new Category { Name = "Přivýdělek", ColorHex = "#4DB6AC", IconKey = "Bank" });
            Categories.Add(new Category { Name = "Jídlo", ColorHex = "#FF5722", IconKey = "Food" });
            Categories.Add(new Category { Name = "Bydlení", ColorHex = "#4CAF50", IconKey = "Home" });
            Categories.Add(new Category { Name = "Doprava", ColorHex = "#2196F3", IconKey = "Car" });
            Categories.Add(new Category { Name = "Káva", ColorHex = "#795548", IconKey = "Coffee" });
            Categories.Add(new Category { Name = "Nákupy", ColorHex = "#FF9800", IconKey = "Shopping" });
            Categories.Add(new Category { Name = "Oblečení", ColorHex = "#9C27B0", IconKey = "Clothes" });
            Categories.Add(new Category { Name = "Zábava", ColorHex = "#E91E63", IconKey = "Entertainment" });
            Categories.Add(new Category { Name = "Cestování", ColorHex = "#00BCD4", IconKey = "Travel" });
            Categories.Add(new Category { Name = "Dárky", ColorHex = "#F06292", IconKey = "Gifts" });
            Categories.Add(new Category { Name = "Zdraví", ColorHex = "#F44336", IconKey = "Health" });
            Categories.Add(new Category { Name = "Vzdělání", ColorHex = "#3F51B5", IconKey = "Education" });
            Categories.Add(new Category { Name = "Telefon", ColorHex = "#607D8B", IconKey = "Phone" });
            Categories.Add(new Category { Name = "Internet", ColorHex = "#009688", IconKey = "Internet" });
            Categories.Add(new Category { Name = "Mazlíčci", ColorHex = "#FF7043", IconKey = "Pets" });

            SaveData();
        }

        /// <summary>
        /// Smaže veškerá data uživatele a uvede databázi do výchozího stavu (tovární nastavení).
        /// </summary>
        public void ResetFactoryDefaults()
        {
            // Fyzické smazání záznamů (SQLite s EF Core neposkytuje lepší či rychlejší variantu pro TRUNCATE)
            Transactions.RemoveRange(Transactions);
            Categories.RemoveRange(Categories);
            AppSettings.RemoveRange(AppSettings);
            
            SaveChanges();

            // Obnova výchozího nezbytného stavu
            Settings = new ApplicationSettings();
            AppSettings.Add(Settings);
            CreateDefaultCategories();
        }
    }
}
