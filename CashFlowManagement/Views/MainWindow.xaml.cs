using CashFlowManagement.Services;
using CashFlowManagement.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace CashFlowManagement.Views
{
    /// <summary>
    /// Main window of the application providing the user interface for managing financial transactions.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// Sets up the data context and initializes required services.
        /// </summary>
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

        /// <summary>
        /// Handles the window loaded event, prompting the user to load previous transactions.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
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

        /// <summary>
        /// Handles the edit menu item click event, enabling edit mode for the selected transaction.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsEditMode = true;
            }
        }

        /// <summary>
        /// Handles the window closing event, prompting the user to save changes before closing.
        /// </summary>
        /// <param name="e">Event arguments containing the cancellation option.</param>
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
