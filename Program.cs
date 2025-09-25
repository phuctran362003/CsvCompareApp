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
                        Console.WriteLine("\n🔍 Đang phân tích file CSV...");
                        var (hasHeader, availableColumns) = userInterfaceService.AnalyzeCsvFile(filePath);

                        if (availableColumns.Any())
                        {
                            Console.WriteLine("\n📋 Các cột phát hiện trong file:");
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
                            var (column1, column2) = userInterfaceService.SelectTwoColumns(availableColumns);
                            
                            columnConfig = new ColumnConfiguration
                            {
                                FilePath = filePath,
                                HasHeaderRecord = hasHeader,
                                ColumnNames = new List<string> { column1, column2 }
                            };

                            var records = csvReaderService.ReadCsvFile(columnConfig);
                            result = comparisonService.CompareTwoColumns(records, column1, column2);
                        }
                        else
                        {
                            var (groupA, groupB) = userInterfaceService.SelectGroupColumns(availableColumns);
                            
                            columnConfig = new ColumnConfiguration
                            {
                                FilePath = filePath,
                                HasHeaderRecord = hasHeader,
                                ColumnNames = new List<string> 
                                { 
                                    groupA.IdColumn, groupA.AmountColumn,
                                    groupB.IdColumn, groupB.AmountColumn 
                                }
                            };

                            var records = csvReaderService.ReadCsvFile(columnConfig);
                            result = comparisonService.CompareGroupColumns(records, groupA, groupB);
                        }

                        // 6. In tóm tắt
                        comparisonService.PrintSummary(result, comparisonType);
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

                Console.WriteLine("\nCảm ơn bạn đã sử dụng CSV Comparison Tool!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                Console.WriteLine("Nhấn Enter để thoát...");
                Console.ReadLine();
            }
        }
    }
}
