using System;

namespace SpravaOsobnichFinanci.ViewModels
{
    /// <summary>
    /// Pomocná datová třída (DTO) reprezentující jeden řádek detailní legendy u koláčových grafů na Dashboardu.
    /// Využívá se čistě pro usnadnění bindingu v uživatelském rozhraní.
    /// </summary>
    internal class ChartLegendItem
    {
        // Privátní fields pro ukládání hodnot
        private string _categoryName = string.Empty;
        private string _icon = string.Empty;
        private string _colorHex = string.Empty;
        private decimal _amount;
        private string _percentageText = string.Empty;

        /// Veřejné Properties pro UI Binding
        public string CategoryName 
        { 
            get => _categoryName; 
            set => _categoryName = value; 
        }

        /// <summary>
        /// Textový klíč ikony (např. 'FastFood'), který je v XAMLu okamžitě transformován pomocí IconConverteru.
        /// </summary>
        public string Icon 
        { 
            get => _icon; 
            set => _icon = value; 
        }

        public string ColorHex 
        { 
            get => _colorHex; 
            set => _colorHex = value; 
        }

        public decimal Amount 
        { 
            get => _amount; 
            set => _amount = value; 
        }

        /// <summary>
        /// Formátovaný text s procentuálním podílem (např. "25 %"), který se zobrazuje vedle částky v legendě.
        /// </summary>
        public string PercentageText 
        { 
            get => _percentageText; 
            set => _percentageText = value; 
        }
    }
}
