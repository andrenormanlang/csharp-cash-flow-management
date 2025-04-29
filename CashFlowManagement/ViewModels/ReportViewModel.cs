using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using CashFlowManagement.Models;

namespace CashFlowManagement.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private string _reportTitle = string.Empty;
        private decimal _totalRevenue;
        private decimal _totalExpenses;
        private decimal _netCashFlow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                _reportTitle = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                _totalExpenses = value;
                OnPropertyChanged();
            }
        }

        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set
            {
                _netCashFlow = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MainViewModel.CategorySummary> TopExpenses { get; } = new();
        public ObservableCollection<MainViewModel.CategorySummary> TopRevenues { get; } = new();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}