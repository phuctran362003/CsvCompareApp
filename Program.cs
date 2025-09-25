using System.Text;
using CsvCompareApp.Models;
using CsvCompareApp.Services;

namespace CsvCompareApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Đảm bảo Console hỗ trợ UTF-8 cho tiếng Việt
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            Console.WriteLine("Bắt đầu chương trình so sánh CSV...\n");
            
            var csvReaderService = new CsvReaderService();
            var comparisonService = new ComparisonService();
            var userInterfaceService = new UserInterfaceService(csvReaderService);

            try
            {
                do
                {
                    try
                    {
                        // 1. Lấy đường dẫn file
                        string filePath = userInterfaceService.GetFilePath();

                        // 2. Tự động phân tích file CSV
                        Console.WriteLine("\nĐang phân tích file...");
                        var (hasHeader, availableColumns) = userInterfaceService.AnalyzeCsvFile(filePath);

                        if (availableColumns.Any())
                        {
                            Console.WriteLine("\nCác cột phát hiện trong file:");
                            for (int i = 0; i < availableColumns.Count; i++)
                            {
                                Console.WriteLine($"  {i + 1}. {availableColumns[i]}");
                            }
                        }

                        // 3. Chọn loại so sánh
                        ComparisonType comparisonType = userInterfaceService.GetComparisonType();

                        // 4. Cấu hình cột dựa trên loại so sánh
                        ColumnConfiguration columnConfig;
                        ComparisonResult result;

                        if (comparisonType == ComparisonType.TwoColumns)
                        {
                            var (column1, column2) = userInterfaceService.ConfigureTwoColumns(availableColumns);
                            
                            columnConfig = new ColumnConfiguration
                            {
                                FilePath = filePath,
                                HasHeaderRecord = hasHeader,
                                ColumnNames = availableColumns
                            };

                            var records = csvReaderService.ReadCsvFile(columnConfig);
                            var result = comparisonService.CompareTwoColumns(records, column1, column2);
                            userInterfaceService.DisplayTwoColumnResult(result);
                        }
                        else // TwoGroups
                        {
                            var groupConfig = userInterfaceService.ConfigureGroup(filePath, availableColumns);
                            
                            columnConfig = new ColumnConfiguration
                            {
                                FilePath = filePath,
                                HasHeaderRecord = hasHeader,
                                ColumnNames = availableColumns
                            };

                            var records = csvReaderService.ReadCsvFile(columnConfig);
                            var result = comparisonService.CompareGroupColumns(records, groupConfig);
                            comparisonService.PrintGroupSummary(result);
                        }
                    }
                    catch (Exception loopEx)
                    {
                        Console.WriteLine($"Lỗi trong quá trình xử lý: {loopEx.Message}");
                        Console.WriteLine("Bạn có muốn thử lại không? (y/n): ");
                        var retry = Console.ReadLine();
                        if (retry?.ToLower() != "y")
                            break;
                    }

                } while (userInterfaceService.AskToContinue());

                Console.WriteLine("\nCảm ơn bạn đã sử dụng công cụ so sánh!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nLỗi nghiêm trọng: {ex.Message}");
                Console.WriteLine("Chương trình sẽ thoát.");
            }
        }
    }
}
