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
    public class JsonDataService : IDataService
    {
        private string _dataFile;

        public JsonDataService(string dataFile = "transactions.json")
        {
            _dataFile = dataFile;
        }

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
