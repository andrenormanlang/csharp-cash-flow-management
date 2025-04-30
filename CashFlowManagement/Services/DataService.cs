using CashFlowManagement.Services.Interfaces;
using CashFlowManagement.Interfaces;
using CashFlowManagement.Models;
using System.Text.Json;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Services
{
    /// <summary>
    /// Provides functionality for persisting and retrieving financial transaction data in JSON format.
    /// Implements IDataService interface.
    /// </summary>
    public class JsonDataService : IDataService
    {
        private string _dataFile;

        /// <summary>
        /// Initializes a new instance of the JsonDataService class.
        /// </summary>
        /// <param name="dataFile">The name of the JSON file to use for data storage. Defaults to "transactions.json".</param>
        public JsonDataService(string dataFile = "transactions.json")
        {
            _dataFile = dataFile;
        }

        /// <summary>
        /// Saves a collection of transactions to a JSON file asynchronously.
        /// Prompts the user to select a save location.
        /// </summary>
        /// <param name="transactions">The collection of transactions to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        /// <exception cref="Exception">Thrown when the save operation fails.</exception>
        public async Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = ".json",
                    FileName = "transactions.json"
                };

                if (dialog.ShowDialog() == true)
                {
                    _dataFile = dialog.FileName;
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true
                    };

                    var json = JsonSerializer.Serialize(transactions.ToList(), options);
                    await File.WriteAllTextAsync(_dataFile, json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// Loads transactions from a JSON file asynchronously.
        /// Prompts the user to select a file to load.
        /// </summary>
        /// <returns>A task containing the loaded collection of transactions.</returns>
        /// <exception cref="Exception">Thrown when the load operation fails.</exception>
        public async Task<IEnumerable<ITransaction>> LoadTransactionsAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = ".json"
                };

                if (dialog.ShowDialog() == true)
                {
                    _dataFile = dialog.FileName;
                    if (File.Exists(_dataFile))
                    {
                        var json = await File.ReadAllTextAsync(_dataFile);
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            return Enumerable.Empty<ITransaction>();
                        }

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        try
                        {
                            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, options);
                            return transactions ?? Enumerable.Empty<ITransaction>();
                        }
                        catch
                        {
                            // Fallback to manual parsing if direct deserialization fails
                            var jsonDocument = JsonDocument.Parse(json);
                            var transactions = new List<ITransaction>();

                            foreach (var element in jsonDocument.RootElement.EnumerateArray())
                            {
                                var transaction = new Transaction
                                {
                                    Date = element.GetProperty("Date").GetDateTime(),
                                    Amount = element.GetProperty("Amount").GetDecimal(),
                                    Description = element.GetProperty("Description").GetString() ?? string.Empty,
                                    Category = new Category
                                    {
                                        Name = element.GetProperty("Category").GetProperty("Name").GetString() ?? string.Empty,
                                        Type = (CategoryType)element.GetProperty("Category").GetProperty("Type").GetInt32(),
                                        TransactionCategory = (TransactionCategory)element.GetProperty("Category").GetProperty("TransactionCategory").GetInt32()
                                    }
                                };
                                transactions.Add(transaction);
                            }

                            return transactions;
                        }
                    }
                }
                return Enumerable.Empty<ITransaction>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
