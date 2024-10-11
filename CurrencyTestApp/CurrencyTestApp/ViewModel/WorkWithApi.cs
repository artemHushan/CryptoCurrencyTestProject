using CurrencyTestApp.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CurrencyTestApp.ViewModel
{
    public static class WorkWithApi
    {
        public static async Task LoadCryptocurrencyData(ObservableCollection<CryptoCurrencyViewInfo> Cryptocurrencies,
            ObservableCollection<CryptoCurrencyViewInfo> CryptocurrenciesTop10, ObservableCollection<CryptoCurrencyViewInfo> CryptocurrenciesBySearch)
        {
            await GetCryptocurrencyMapData(Cryptocurrencies);
            await GetCryptocurrencyMetaData(Cryptocurrencies);
            await UpdateHistoricalDataAsync(Cryptocurrencies, CryptocurrenciesTop10);
            await ForSearch(Cryptocurrencies, CryptocurrenciesBySearch);
        }

        public static async Task  ForSearch(ObservableCollection<CryptoCurrencyViewInfo> Cryptocurrencies, ObservableCollection<CryptoCurrencyViewInfo> CryptocurrenciesBySearch)
        {
            foreach (CryptoCurrencyViewInfo item in Cryptocurrencies)
            {
                CryptocurrenciesBySearch.Add(item);
            }
        }
        private static async Task GetCryptocurrencyMapData(
            ObservableCollection<CryptoCurrencyViewInfo> Cryptocurrencies)
        {
            string apiKey = "4e8b67c4-f3b3-45fe-8a29-d71af84806f3"; // Замініть на ваш API ключ
            string mapUrl = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/map?listing_status=active";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                HttpResponseMessage response = await client.GetAsync(mapUrl);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);
                    foreach (var item in json["data"]) // Використовуємо Take(10) для обмеження до 10 валют
                    {
                        Cryptocurrencies.Add(new CryptoCurrencyViewInfo
                        {
                            Id = int.Parse(item["id"].ToString()), // Зберігаємо Id
                            Name = item["name"].ToString(),
                            Rank = int.Parse(item["rank"].ToString()),
                            Symbol = item["symbol"].ToString(),
                            Slug = item["slug"].ToString(),
                            IsActive = item["is_active"].ToString() == "1",
                        });
                    }
                }
            }
        }

        // Метод для отримання додаткових метаданих (логотипів і URL)
        private static async Task GetCryptocurrencyMetaData(ObservableCollection<CryptoCurrencyViewInfo> Cryptocurrencies)
        {
            string apiKey = "4e8b67c4-f3b3-45fe-8a29-d71af84806f3"; // Ваш API ключ
            int batchSize = 1000; // Кількість криптовалют, які запитуються за раз
            for (int i = 0; i < Cryptocurrencies.Count; i += batchSize)
            {
                var batch = Cryptocurrencies.Skip(i).Take(batchSize);
                string idsString = string.Join(",", batch.Select(c => c.Id));
                if (string.IsNullOrWhiteSpace(idsString))
                {
                    continue;
                }
                string infoUrl = $"https://pro-api.coinmarketcap.com/v2/cryptocurrency/info?id={idsString}";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                    HttpResponseMessage infoResponse = await client.GetAsync(infoUrl);
                    if (infoResponse.IsSuccessStatusCode)
                    {
                        string infoResult = await infoResponse.Content.ReadAsStringAsync();
                        JObject infoJson = JObject.Parse(infoResult);
                        foreach (var currency in batch)
                        {
                            var currencyData = infoJson["data"]
                                .FirstOrDefault(item => item.First()["id"].ToString() == currency.Id.ToString());
                            if (currencyData != null)
                            {
                                string logoUrl = currencyData.First()["logo"].ToString();
                                string websiteUrl = currencyData.First()["urls"]["website"].FirstOrDefault()
                                    ?.ToString();
                                currency.Logo = new BitmapImage(new Uri(logoUrl));
                                currency.ResourceUrl = websiteUrl;
                                if (currency.Rank >= 1 && currency.Rank <= 10)
                                {
                                   
                                }
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = await infoResponse.Content.ReadAsStringAsync();
                    }
                }

            }

        }

        private static async Task UpdateHistoricalDataAsync(ObservableCollection<CryptoCurrencyViewInfo> cryptocurrencies, ObservableCollection<CryptoCurrencyViewInfo> CryptocurrenciesTop10)
        {
            string apiKey = "4e8b67c4-f3b3-45fe-8a29-d71af84806f3"; // Ваш API ключ
            string historicalUrl = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest"; // Запит на останній знімок ринку
            int start = 1;
            int limit = 1000;
            bool hasMoreData = true;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                while (hasMoreData)
                {
                    string requestUrl = $"{historicalUrl}?start={start}&limit={limit}";
                    HttpResponseMessage response = await client.GetAsync(requestUrl); // Запит до API
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(result);
                        foreach (JToken item in json["data"])
                        {
                            int currencyId = int.Parse(item["id"].ToString());
                            var foundCurrency = cryptocurrencies.FirstOrDefault(crypto => crypto.Id == currencyId);
                            if (foundCurrency != null)
                            {
                                foundCurrency.Name = item["name"].ToString();
                                foundCurrency.Rank = int.Parse(item["cmc_rank"].ToString());
                                foundCurrency.HistoricalInfo.ReleaseDate = DateTime.Parse(item["date_added"].ToString());
                                foundCurrency.HistoricalInfo.MaxSupply = item["max_supply"]?.ToObject<double?>() ?? 0;
                                foundCurrency.HistoricalInfo.TotalSupply = item["total_supply"]?.ToObject<double?>() ?? 0;
                                var quote = item["quote"]?["USD"];
                                if (quote != null)
                                {
                                    foundCurrency.MarketInfo.Price = Math.Round(quote["price"]?.ToObject<double?>() ?? 0, 6);
                                    foundCurrency.MarketInfo.PercentChange24h = Math.Round(quote["percent_change_24h"]?.ToObject<double?>() ?? 0, 2);
                                    foundCurrency.MarketInfo.Volume24h = quote["volume_24h"]?.ToObject<double?>() ?? 0;
                                    foundCurrency.MarketInfo.MarketCap = quote["market_cap"]?.ToObject<double?>() ?? 0;
                                }

                                if (foundCurrency.Rank <= 10)
                                {
                                    CryptocurrenciesTop10.Add(foundCurrency);
                                }
                            }
                        }

                        int dataCount = json["data"].Count();
                        if (dataCount < limit)
                        {
                            hasMoreData = false; // Більше немає сторінок
                        }
                        start += limit;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}