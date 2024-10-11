using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyTestApp.Model
{
    public interface ICryptocurrency
    {
        public int Id { get; set; } // Додаємо поле Id
        string Name { get; set; }  
        int Rank { get; set; }
    }
}
