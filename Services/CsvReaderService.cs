using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvCompareApp.Models;

namespace CsvCompareApp.Services
{
    public class CsvReaderService
    {
        public List<Dictionary<string, object>> ReadCsvFile(ColumnConfiguration config)
        {
            try
            {
                using var reader = new StreamReader(config.FilePath);
                
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = config.HasHeaderRecord
                };
                
                using var csv = new CsvReader(reader, csvConfig);
                
                var records = new List<Dictionary<string, object>>();
                
                if (config.HasHeaderRecord)
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    while (csv.Read())
                    {
                        var record = new Dictionary<string, object>();
                        foreach (var columnName in config.ColumnNames)
                        {
                            try
                            {
                                var value = csv.GetField(columnName);
                                record[columnName] = value ?? "";
                            }
                            catch
                            {
                                record[columnName] = "";
                            }
                        }
                        records.Add(record);
                    }
                }
                else
                {
                    while (csv.Read())
                    {
                        var record = new Dictionary<string, object>();
                        for (int i = 0; i < config.ColumnNames.Count && i < csv.ColumnCount; i++)
                        {
                            var value = csv.GetField(i);
                            record[config.ColumnNames[i]] = value ?? "";
                        }
                        records.Add(record);
                    }
                }
                
                return records;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc file CSV: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }

        public List<string> GetCsvHeaders(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                
                csv.Read();
                csv.ReadHeader();
                
                return csv.HeaderRecord?.ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc header CSV: {ex.Message}");
                return new List<string>();
            }
        }
    }
}