using CurrencyTestApp.Model;
using CurrencyTestApp.ViewModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace CurrencyTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
          
            InitializeComponent();
           

        }
        // Метод для відкриття посилань у браузері
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        private void CryptoTextBoxFirst_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Перевіряємо, чи є введений символ цифрою
            e.Handled = !char.IsDigit(e.Text, 0);
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is CryptoCurrencyViewInfo selectedItem)
            {
                var viewModel = this.DataContext as MainViewModel;
                viewModel?.HandleDataGridDoubleClick(selectedItem); // Виклик методу у ViewModel
            }
        }


    }
}