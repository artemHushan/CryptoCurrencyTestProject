using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyTestApp.Model
{
    public class CryptoMarketInfo:ICryptocurrency
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public double Price { get; set; }                    
        public double MarketCap { get; set; }
        public double Volume24h { get; set; }            
        public double PercentChange24h { get; set; }
    }
}
