using CashFlowManagement.Services.Interfaces;
using CashFlowManagement.Interfaces;
using CashFlowManagement.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Services
{
    public class TransactionConverter : JsonConverter<ITransaction>
    {
        public override ITransaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var transaction = new Transaction
            {
                Date = root.GetProperty("Date").GetDateTime(),
                Amount = root.GetProperty("Amount").GetDecimal(),
                Description = root.GetProperty("Description").GetString() ?? string.Empty,
                Category = new Category
                {
                    Name = root.GetProperty("Category").GetProperty("Name").GetString() ?? string.Empty,
                    Type = (CategoryType)root.GetProperty("Category").GetProperty("Type").GetInt32(),
                    TransactionCategory = (TransactionCategory)root.GetProperty("Category").GetProperty("TransactionCategory").GetInt32()
                }
            };

            return transaction;
        }

        public override void Write(Utf8JsonWriter writer, ITransaction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    public class JsonDataService : IDataService
    {
        private string _dataFile;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataService(string dataFile = "transactions.json")
        {
            _dataFile = dataFile;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _jsonOptions.Converters.Add(new TransactionConverter());
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
                    var json = JsonSerializer.Serialize(transactions, _jsonOptions);
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

                        var transactions = JsonSerializer.Deserialize<List<ITransaction>>(json, _jsonOptions);
                        return transactions ?? Enumerable.Empty<ITransaction>();
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
