using CashFlowManagement.Services;
using CashFlowManagement.ViewModels;
using CashFlowManagement.Views;
using System.Windows;

namespace CashFlowManagement
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dataService = new JsonDataService();
            var transactionManager = new TransactionManager();
            var viewModel = new MainViewModel(transactionManager, dataService);

            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.Show();
        }
    }
}