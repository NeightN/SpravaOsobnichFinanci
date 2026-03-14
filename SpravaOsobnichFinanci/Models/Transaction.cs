using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpravaOsobnichFinanci.Models
{
    /// <summary>
    /// Reprezentuje jednotlivou finanční transakci (příjem nebo výdaj).
    /// Jádro celého modelu pro evidenci financí.
    /// </summary>
    internal class Transaction
    {
        private Guid _id = Guid.NewGuid();
        private DateTime _date;
        private string _description = string.Empty;
        private decimal _amount;
        private TransactionType _type;
        private Guid _categoryId;
        private Category? _transactionCategory;

        /// <summary>
        /// Unikátní identifikátor transakce. 
        /// Init setter zabraňuje změně ID po vytvoření instance.
        /// </summary>
        public Guid Id 
        { 
            get => _id; 
            init => _id = value; 
        }

        // Především datum transakce (např. 01.01.2025)
        // Může být použito pro řazení, filtrování a zobrazení v kalendáři
        public DateTime Date 
        { 
            get => _date; 
            set => _date = value; 
        }
        
        // Textová poznámka pro uživatele (např. "Nákup potravin")
        public string Description 
        { 
            get => _description; 
            set => _description = value; 
        }
        
        // Částka transakce
        public decimal Amount 
        { 
            get => _amount; 
            set => _amount = value; 
        }

        // Určuje, zda se jedná o příjem (Income) nebo výdaj (Expense)
        public TransactionType Type 
        { 
            get => _type; 
            set => _type = value; 
        }

        // === Vazba na kategorii (EF Core) ===

        // Cizí klíč pro kategorii, ke které transakce patří
        public Guid CategoryId 
        { 
            get => _categoryId; 
            set => _categoryId = value; 
        }

        // Navigační vlastnost pro načtení samotného objektu kategorie z DB.
        // Atribut ForeignKey explicitně určuje, který cizí klíč má EF Core použít.
        [ForeignKey(nameof(CategoryId))]
        public Category? TransactionCategory 
        { 
            get => _transactionCategory; 
            set => _transactionCategory = value; 
        }
    }
}
