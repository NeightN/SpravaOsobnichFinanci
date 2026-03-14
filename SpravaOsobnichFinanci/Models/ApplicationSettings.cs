using System;

namespace SpravaOsobnichFinanci.Models
{
    /// <summary>
    /// Uchovává globální nastavení aplikace a uživatelské preference 
    /// (např. nastavení měny nebo rozložení dashboardu).
    /// </summary>
    internal class ApplicationSettings
    {
        private int _id = 1;
        private string _currencySymbol = "Kč";
        private bool _isBalanceWidgetVisible = true;
        private bool _isCashFlowWidgetVisible = true;
        private bool _isIncomeWidgetVisible = true;
        private bool _isExpenseWidgetVisible = true;

        /// <summary>
        /// Primární klíč pro uložené nastavení v databázi. 
        /// Aplikace by měla vždy pracovat pouze s jedním záznamem (Id = 1).
        /// </summary>
        public int Id 
        { 
            get => _id; 
            set => _id = value; 
        }

        /// <summary>
        /// Znak nebo zkratka měny používaná napříč aplikací (např. Kč, €).
        /// </summary>
        public string CurrencySymbol 
        { 
            get => _currencySymbol; 
            set => _currencySymbol = value; 
        }

        // --- Viditelnost widgetů na hlavní obrazovce ---
        // Uživatel může skrýt nebo zobrazit jednotlivé widgety (Zůstatek, Cash Flow, Příjmy, Výdaje)

        public bool IsBalanceWidgetVisible 
        { 
            get => _isBalanceWidgetVisible; 
            set => _isBalanceWidgetVisible = value; 
        }
        
        public bool IsCashFlowWidgetVisible 
        { 
            get => _isCashFlowWidgetVisible; 
            set => _isCashFlowWidgetVisible = value; 
        }
        
        public bool IsIncomeWidgetVisible 
        { 
            get => _isIncomeWidgetVisible; 
            set => _isIncomeWidgetVisible = value; 
        }
        
        public bool IsExpenseWidgetVisible 
        { 
            get => _isExpenseWidgetVisible; 
            set => _isExpenseWidgetVisible = value; 
        }
    }
}
