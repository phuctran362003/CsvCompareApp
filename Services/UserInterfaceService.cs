using CsvCompareApp.Models;

namespace CsvCompareApp.Services
{
    public class UserInterfaceService
    {
        private readonly CsvReaderService _csvReaderService;

        public UserInterfaceService(CsvReaderService csvReaderService)
        {
            _csvReaderService = csvReaderService;
        }

        public string GetFilePath()
        {
            Console.WriteLine("=== CSV COMPARISON TOOL ===\n");
            
            while (true)
            {
                Console.Write("Nhập đường dẫn đến file CSV: ");
                string? filePath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Vui lòng nhập đường dẫn file!");
                    continue;
                }
                
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File không tồn tại! Vui lòng kiểm tra lại đường dẫn.");
                    continue;
                }
                
                return filePath;
            }
        }

        public ComparisonType GetComparisonType()
        {
            Console.WriteLine("\nChọn loại so sánh:");
            Console.WriteLine("1. So sánh hai cột đơn (A vs B)");
            Console.WriteLine("2. So sánh nhóm cột (ID + Amount)");
            
            while (true)
            {
                Console.Write("Nhập lựa chọn (1 hoặc 2): ");
                string? input = Console.ReadLine();
                
                if (input == "1")
                    return ComparisonType.TwoColumns;
                else if (input == "2")
                    return ComparisonType.GroupColumns;
                else
                    Console.WriteLine("Vui lòng nhập 1 hoặc 2!");
            }
        }

        public bool GetHasHeaderOption()
        {
            Console.WriteLine("\nFile CSV có header (dòng tiêu đề) không?");
            Console.WriteLine("1. Có header");
            Console.WriteLine("2. Không có header");
            
            while (true)
            {
                Console.Write("Nhập lựa chọn (1 hoặc 2): ");
                string? input = Console.ReadLine();
                
                if (input == "1")
                    return true;
                else if (input == "2")
                    return false;
                else
                    Console.WriteLine("Vui lòng nhập 1 hoặc 2!");
            }
        }

        public (string, string) SelectTwoColumns(List<string> availableColumns, bool hasHeader)
        {
            if (hasHeader && availableColumns.Any())
            {
                Console.WriteLine("\nCác cột có sẵn:");
                for (int i = 0; i < availableColumns.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableColumns[i]}");
                }
                
                string column1 = SelectColumn(availableColumns, "Chọn cột thứ nhất");
                string column2 = SelectColumn(availableColumns, "Chọn cột thứ hai");
                
                return (column1, column2);
            }
            else
            {
                Console.Write("Nhập tên cột thứ nhất (hoặc A, B, C...): ");
                string? col1 = Console.ReadLine();
                Console.Write("Nhập tên cột thứ hai (hoặc A, B, C...): ");
                string? col2 = Console.ReadLine();
                
                return (col1 ?? "A", col2 ?? "B");
            }
        }

        public (GroupColumnConfiguration, GroupColumnConfiguration) SelectGroupColumns(List<string> availableColumns, bool hasHeader)
        {
            Console.WriteLine("\n=== CẤU HÌNH NHÓM 1 ===");
            var group1 = ConfigureGroup(availableColumns, hasHeader, "Nhóm 1");
            
            Console.WriteLine("\n=== CẤU HÌNH NHÓM 2 ===");
            var group2 = ConfigureGroup(availableColumns, hasHeader, "Nhóm 2");
            
            return (group1, group2);
        }

        private GroupColumnConfiguration ConfigureGroup(List<string> availableColumns, bool hasHeader, string groupName)
        {
            var config = new GroupColumnConfiguration { GroupName = groupName };
            
            if (hasHeader && availableColumns.Any())
            {
                Console.WriteLine("Các cột có sẵn:");
                for (int i = 0; i < availableColumns.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableColumns[i]}");
                }
                
                config.IdColumn = SelectColumn(availableColumns, $"Chọn cột ID cho {groupName}");
                config.AmountColumn = SelectColumn(availableColumns, $"Chọn cột Amount cho {groupName}");
            }
            else
            {
                Console.Write($"Nhập tên cột ID cho {groupName}: ");
                config.IdColumn = Console.ReadLine() ?? "";
                
                Console.Write($"Nhập tên cột Amount cho {groupName}: ");
                config.AmountColumn = Console.ReadLine() ?? "";
            }
            
            return config;
        }

        private string SelectColumn(List<string> availableColumns, string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (số thứ tự hoặc tên cột): ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Vui lòng nhập lựa chọn!");
                    continue;
                }
                
                // Thử parse số
                if (int.TryParse(input, out int index) && index >= 1 && index <= availableColumns.Count)
                {
                    return availableColumns[index - 1];
                }
                
                // Thử tìm theo tên
                var matchingColumn = availableColumns.FirstOrDefault(c => 
                    c.Equals(input, StringComparison.OrdinalIgnoreCase));
                
                if (matchingColumn != null)
                {
                    return matchingColumn;
                }
                
                Console.WriteLine("Lựa chọn không hợp lệ! Vui lòng thử lại.");
            }
        }

        public bool AskToContinue()
        {
            Console.WriteLine("\nBạn có muốn thực hiện so sánh khác không?");
            Console.WriteLine("1. Có");
            Console.WriteLine("2. Không");
            
            while (true)
            {
                Console.Write("Nhập lựa chọn (1 hoặc 2): ");
                string? input = Console.ReadLine();
                
                if (input == "1")
                    return true;
                else if (input == "2")
                    return false;
                else
                    Console.WriteLine("Vui lòng nhập 1 hoặc 2!");
            }
        }
    }
}