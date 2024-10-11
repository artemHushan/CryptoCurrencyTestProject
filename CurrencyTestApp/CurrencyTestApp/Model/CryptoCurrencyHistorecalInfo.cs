using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyTestApp.Model
{
    public  class CryptoCurrencyHistoricalInfo:ICryptocurrency
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public DateTime ReleaseDate { get; set; } = new DateTime();
        public double MaxSupply { get; set; }                
        public double TotalSupply { get; set; }
    }
}
