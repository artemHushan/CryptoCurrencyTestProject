using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CurrencyTestApp.Model
{
    public class CryptoCurrencyViewInfo : ICryptocurrency
    {
        public int Id { get; set; } // Додаємо поле Id
        public string Name { get; set; }
        public CryptoMarketInfo MarketInfo { get; set; } = new CryptoMarketInfo();
        public CryptoCurrencyHistoricalInfo HistoricalInfo { get; set; } = new CryptoCurrencyHistoricalInfo();
        public int Rank { get; set; }
        public string Symbol { get; set; }
        public string Slug { get; set; }
        public ImageSource Logo { get; set; }
        public bool IsActive { get; set; }
        public string ResourceUrl { get; set; }
    }

}
