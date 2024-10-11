using CurrencyTestApp.Model;
using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Mime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.Windows.Data;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System.Windows.Input;
using DevExpress.Mvvm.UI;

namespace CurrencyTestApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public void SetTheme(string theme)
        {
            var dict = new ResourceDictionary();
            switch (theme)
            {
                case "Light":
                    dict.Source = new Uri("LightTheme.xaml", UriKind.Relative);
                    break;
                case "Dark":
                    dict.Source = new Uri("DarkTheme.xaml", UriKind.Relative);
                    break;
            }
            // Очищення старих ресурсів і додавання нових
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
        public string SelectedCurrencyName { get; set; }
        public CryptoCurrencyViewInfo DetailsInfo { get; set; }
        public string Theme { get; set; } = "Dark";
        public bool CheckTheme { get; set; } = false;
        public DelegateCommand SwapThemeCommand { get; set; }
        public Brush HomeBrush { get; set; }
        public Brush AllCurencyBrush { get; set; }
        public Brush ConverterBrush { get; set; }
        public Brush SettingsBrush { get; set; }
        // Словник для зберігання кольорів вкладок
        private Dictionary<string, Brush> DefaultColors = new Dictionary<string, Brush>
        {
            { "Home", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) },
            { "AllCurency", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) },
            { "Converter", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) },
            { "Settings", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC")) }
        };
        public int SelectedTabIndex { get; set; } = 0;
        public static ObservableCollection<CryptoCurrencyViewInfo> Cryptocurrency { get; set; } = new ObservableCollection<CryptoCurrencyViewInfo>();
        public ObservableCollection<CryptoCurrencyViewInfo> CryptocurrencyTop10 { get; set; } = new ObservableCollection<CryptoCurrencyViewInfo>();
        public ObservableCollection<CryptoCurrencyViewInfo> CryptocurrencyBySearch { get; set; } = new ObservableCollection<CryptoCurrencyViewInfo>();
        public static List<CryptoCurrencyViewInfo> CryptocurrencysToExchange {get; set; }=new List<CryptoCurrencyViewInfo>();
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand CryptoCoinsCommand { get; set; }
        public DelegateCommand ConverterCommand { get; set; }
        public DelegateCommand SettingsCommand { get; set; }
        public ImageSource MainImage { get; set; } = new BitmapImage(new Uri("pack://application:,,,/Images/light/homepageNoNActive.png"));
        public string NameOfTabPage { get; set; } = "Home";
        private string _searchText;
        public Visibility SearchVisible { get; set; } = Visibility.Collapsed;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value, nameof(SearchText)))
                {
                    CryptocurrencyBySearch.Clear();
                    UpdateCryptocurrencyBySearch(SearchText);
                }
            }
        }
        private string _cryptoTextFirst;
        public string CryptoTextFirst
        {
            get => _cryptoTextFirst;
            set
            {
                if (SetProperty(ref _cryptoTextFirst, value, nameof(CryptoTextFirst)))
                {
                    OnCryptoTextChanged(); // Call this only if the property was actually changed
                }
            }
        }
        private string _cryptoTextSecond;
        public string CryptoTextSecond
        {
            get => _cryptoTextSecond;
            set
            {
                if (SetProperty(ref _cryptoTextSecond, value, nameof(CryptoTextSecond)))
                {
                    MakeExchangeSecond(SelectedCryptocurrencyFirst, SelectedCryptocurrencySecond); // Call this only if the property was actually changed
                }
            }
        }
        private string _selectedCryptocurrencyFirst;
        public string SelectedCryptocurrencyFirst
        {
            get => _selectedCryptocurrencyFirst;
            set
            {
               if(SetProperty(ref _selectedCryptocurrencyFirst, value,nameof(SelectedCryptocurrencyFirst)));
               OnCryptoTextChanged();
            }
        }
        private string _selectedCryptocurrencySecond;
        public string SelectedCryptocurrencySecond
        {
            get => _selectedCryptocurrencySecond;
            set
            {
                if(SetProperty(ref _selectedCryptocurrencySecond, value, nameof(SelectedCryptocurrencyFirst)));
                OnCryptoTextChanged();
            }
        }
        private void OnCryptoTextChanged()
        {
            MakeExchange(SelectedCryptocurrencyFirst, SelectedCryptocurrencySecond);
        }
        private void MakeExchange(string First, string Second)
        {
            CryptoCurrencyViewInfo firstCrypto = Cryptocurrency.FirstOrDefault(c => c.Name == First);
            CryptoCurrencyViewInfo secondCrypto = Cryptocurrency.FirstOrDefault(c => c.Name == Second);
            if (firstCrypto != null && secondCrypto != null)
            {
                    if (double.TryParse(CryptoTextFirst, out double CryptoFirst))
                    {
                        double FirstPrice = firstCrypto.MarketInfo.Price;
                        double SecondPrice = secondCrypto.MarketInfo.Price;
                        if (FirstPrice > 0 && SecondPrice > 0)
                        {
                            CryptoTextSecond = ((CryptoFirst * FirstPrice) / SecondPrice).ToString();
                        }
                    }
            }
        }
        private void MakeExchangeSecond(string First, string Second)
        {
            CryptoCurrencyViewInfo firstCrypto = Cryptocurrency.FirstOrDefault(c => c.Name == First);
            CryptoCurrencyViewInfo secondCrypto = Cryptocurrency.FirstOrDefault(c => c.Name == Second);
            if (firstCrypto != null && secondCrypto != null)
            {
                    if (double.TryParse(CryptoTextSecond, out double CryptoSecond))
                    {
                        double FirstPrice = firstCrypto.MarketInfo.Price;
                        double SecondPrice = secondCrypto.MarketInfo.Price;
                        CryptoTextFirst = ((CryptoSecond * SecondPrice) / FirstPrice).ToString();
                    }
            }

        }
        public List<CryptoCurrencyViewInfo> SearchByName(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return new List<CryptoCurrencyViewInfo>();
            var filteredList = Cryptocurrency
                .Where(c => c.Name.StartsWith(searchText)) // Пошук по імені
                .ToList();
            return filteredList;
        }
        private void UpdateCryptocurrencyBySearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) // Перевіряємо, чи рядок пустий або складається з пробілів
            {
                foreach (var currency in Cryptocurrency) // Додаємо всі елементи назад до ObservableCollection
                {
                    CryptocurrencyBySearch.Add(currency);
                }
            }
            else
            {
                var searchResults = SearchByName(searchText); // Отримуємо відфільтровані результати
                foreach (var result in searchResults)
                {
                    CryptocurrencyBySearch.Add(result); // Додаємо результати до ObservableCollection
                }
            }
        }
        private void GetCryptoToExchange()
        {
            foreach (CryptoCurrencyViewInfo item in Cryptocurrency)
            {
                if (item.Rank <= 200 && !CryptocurrencysToExchange.Any(c => c.Id == item.Id))
                {
                    CryptocurrencysToExchange.Add(item);
                }
            }
        }
        private void SetBrushes(string activeTab)
        {
            HomeBrush = DefaultColors["Home"];
            AllCurencyBrush = DefaultColors["AllCurency"];
            ConverterBrush = DefaultColors["Converter"];
            SettingsBrush = DefaultColors["Settings"];
            // Активна вкладка отримує свій колір
            switch (activeTab)
            {
                case "Home":
                    HomeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D97ED"));
                    SelectedTabIndex = 0;
                    break;
                case "AllCurency":
                    AllCurencyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D97ED"));
                    SelectedTabIndex = 1;
                    break;
                case "Converter":
                    ConverterBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D97ED"));
                    SelectedTabIndex = 2;
                    break;
                case "Settings":
                    SettingsBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D97ED"));
                    SelectedTabIndex = 3;
                    break;
            }
        }
        public void HandleDataGridDoubleClick(CryptoCurrencyViewInfo selectedItem)
        {
            if (selectedItem != null)
            {
                SelectedCurrencyName = selectedItem.Name; // Зберігаємо значення "Name"
            
                SelectedTabIndex = 4; 
            
                DetailsInfo = Cryptocurrency?.FirstOrDefault(item => item.Name == SelectedCurrencyName);
                SearchVisible = Visibility.Collapsed;
            }
        }
        public MainViewModel()
        {
            SetBrushes("Home");

            HomeCommand = new DelegateCommand(() =>
            {
                SetBrushes("Home");
                MainImage = new BitmapImage(new Uri("pack://application:,,,/Images/light/homepageNoNActive.png"));
                NameOfTabPage = "Home";
                SearchVisible = Visibility.Collapsed;
            });
            CryptoCoinsCommand = new DelegateCommand(() =>
            {
                SetBrushes("AllCurency");
                MainImage = new BitmapImage(new Uri("pack://application:,,,/Images/light/allCurrencyNonActive.png"));
                NameOfTabPage = "CryptoCoins";
                SearchVisible = Visibility.Visible;
            });
            ConverterCommand = new DelegateCommand(() =>
            {
                SetBrushes("Converter");
                MainImage = new BitmapImage(new Uri("pack://application:,,,/Images/light/exchangeNonActive.png"));
                NameOfTabPage = "Converter";
                SearchVisible = Visibility.Collapsed;
                GetCryptoToExchange();


            });
            SettingsCommand = new DelegateCommand(() =>
            {
                SetBrushes("Settings");
                MainImage = new BitmapImage(new Uri("pack://application:,,,/Images/light/settingNonActive.png"));
                NameOfTabPage = "Settings";
                SearchVisible = Visibility.Collapsed;
            });
            SwapThemeCommand = new DelegateCommand(() =>
            {
                if (CheckTheme)
                {
                    Theme = "Dark";
                    CheckTheme = false;
                }
                else
                {
                    Theme = "Light";
                    CheckTheme = true;
                }
                SetTheme(Theme);
            });
            WorkWithApi.LoadCryptocurrencyData(Cryptocurrency, CryptocurrencyTop10, CryptocurrencyBySearch).ContinueWith(_ =>
            {
                UpdateCryptocurrencyBySearch(string.Empty);  // Виклик оновлення пошуку після завантаження
               
            });
        }
    }
}