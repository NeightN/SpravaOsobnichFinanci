using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpravaOsobnichFinanci.Models
{
    /// <summary>
    /// Určuje základní typ finanční transakce (směr toku peněz).
    /// </summary>
    internal enum TransactionType
    {
        // Kladná položka zvyšující zůstatek (např. výplata, dar)
        Income,

        // Záporná položka snižující zůstatek (např. nákup, jídlo)
        Expense
    }
}
