using System.Windows;

namespace CashFlowManagement.Views
{
    /// <summary>
    /// Interaction logic for the monthly financial report window.
    /// Displays detailed financial statistics and summaries.
    /// </summary>
    public partial class ReportWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the ReportWindow class.
        /// </summary>
        public ReportWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the close button click event.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}