using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using CashFlowManagement.Models;

namespace CashFlowManagement.ViewModels
{
    /// <summary>
    /// View model for the financial report window, providing properties for report display and notifications.
    /// </summary>
    public class ReportViewModel : INotifyPropertyChanged
    {
        private string _reportTitle = string.Empty;
        private decimal _totalRevenue;
        private decimal _totalExpenses;
        private decimal _netCashFlow;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the title of the report, typically including the date period.
        /// </summary>
        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                if (_reportTitle != value)
                {
                    _reportTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the total revenue for the report period.
        /// </summary>
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                if (_totalRevenue != value)
                {
                    _totalRevenue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the total expenses for the report period.
        /// </summary>
        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                if (_totalExpenses != value)
                {
                    _totalExpenses = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the net cash flow (revenue minus expenses) for the report period.
        /// </summary>
        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set
            {
                if (_netCashFlow != value)
                {
                    _netCashFlow = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of top expense categories with their amounts and percentages.
        /// </summary>
        public ObservableCollection<MainViewModel.CategorySummary> TopExpenses { get; } = new();

        /// <summary>
        /// Gets the collection of top revenue categories with their amounts and percentages.
        /// </summary>
        public ObservableCollection<MainViewModel.CategorySummary> TopRevenues { get; } = new();

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. If null, the calling member name is used.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}