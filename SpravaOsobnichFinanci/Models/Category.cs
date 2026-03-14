using System;

namespace SpravaOsobnichFinanci.Models
{
    /// <summary>
    /// Reprezentuje uživatelsky definovanou kategorii finančních transakcí (např. "Potraviny", "Mzda").
    /// Slouží k rozřazování transakcí pro lepší přehled a generování grafů.
    /// </summary>
    internal class Category
    {
        private Guid _id = Guid.NewGuid();
        private string _name = string.Empty;
        private string _colorHex = string.Empty;
        private string _iconKey = string.Empty;

        /// <summary>
        /// Unikátní identifikátor kategorie. 
        /// Init setter zabraňuje nechtěné změně ID v průběhu života objektu.
        /// </summary>
        public Guid Id 
        { 
            get => _id; 
            init => _id = value; 
        }
        
        // Uživatelský název kategorie zobrazený v aplikaci (např. "Bydlení")
        public string Name 
        { 
            get => _name; 
            set => _name = value; 
        }
        
        // HEX barva pro vizuální odlišení kategorie v grafech a seznamech (např. "#FF5733")
        public string ColorHex 
        { 
            get => _colorHex; 
            set => _colorHex = value; 
        }
        
        // Cesta nebo klíč k ikoně, aby UI vědělo, jaký symbol má u kategorie vykreslit
        public string IconKey 
        { 
            get => _iconKey; 
            set => _iconKey = value; 
        }
    }
}
