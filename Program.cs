using CsvCompareApp.Models;
using CsvCompareApp.Services;

namespace CsvCompareApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var csvReaderService = new CsvReaderService();
            var comparisonService = new ComparisonService();
            var userInterfaceService = new UserInterfaceService(csvReaderService);

            try
            {
                do
                {
                    // 1. Lấy đường dẫn file
                    string filePath = userInterfaceService.GetFilePath();

                    // 2. Chọn loại so sánh
                    ComparisonType comparisonType = userInterfaceService.GetComparisonType();

                    // 3. Kiểm tra có header không
                    bool hasHeader = userInterfaceService.GetHasHeaderOption();

                    // 4. Đọc header nếu có
                    List<string> availableColumns = new List<string>();
                    if (hasHeader)
                    {
                        availableColumns = csvReaderService.GetCsvHeaders(filePath);
                        if (availableColumns.Any())
                        {
                            Console.WriteLine("\nCác cột phát hiện trong file:");
                            for (int i = 0; i < availableColumns.Count; i++)
                            {
                                Console.WriteLine($"  {i + 1}. {availableColumns[i]}");
                            }
                        }
                    }

                    // 5. Cấu hình cột dựa trên loại so sánh
                    ColumnConfiguration columnConfig;
                    ComparisonResult result;

                    if (comparisonType == ComparisonType.TwoColumns)
                    {
                        var (column1, column2) = userInterfaceService.SelectTwoColumns(availableColumns, hasHeader);
                        
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
                        var (groupA, groupB) = userInterfaceService.SelectGroupColumns(availableColumns, hasHeader);
                        
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
