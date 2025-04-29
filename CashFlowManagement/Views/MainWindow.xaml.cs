using CashFlowManagement.Services;
using CashFlowManagement.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace CashFlowManagement.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            var dataService = new JsonDataService();
            var transactionManager = new TransactionManager();

            // Create and set ViewModel
            _viewModel = new MainViewModel(transactionManager, dataService);
            DataContext = _viewModel;

            // Load initial data
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Would you like to load previous transactions?",
                                      "Load Data",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _viewModel.LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show("Would you like to save your changes?",
                                      "Save Changes",
                                      MessageBoxButton.YesNoCancel,
                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _viewModel.SaveDataAsync();
                }
                catch (Exception ex)
                {
                    var errorResult = MessageBox.Show(
                        $"Error saving data: {ex.Message}\nDo you want to close anyway?",
                        "Error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (errorResult == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }
    }
}
