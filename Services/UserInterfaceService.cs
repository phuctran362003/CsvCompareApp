using System.Text;
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
            Console.WriteLine("=== CÔNG CỤ SO SÁNH CSV ===");
            Console.WriteLine("📝 Hỗ trợ UTF-8, Windows-1258 và các encoding Tiếng Việt\n");
            
            int attempts = 0;
            const int maxAttempts = 5;
            
            while (attempts < maxAttempts)
            {
                Console.Write("Nhập đường dẫn đến file CSV (hoặc 'exit' để thoát): ");
                string? filePath = Console.ReadLine();
                
                if (string.IsNullOrEmpty(filePath))
                {
                    attempts++;
                    Console.WriteLine("Input trống! Vui lòng nhập đường dẫn file.");
                    continue;
                }
                
                filePath = filePath.Trim();
                
                if (filePath.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Thoát chương trình...");
                    Environment.Exit(0);
                }
                
                // Loại bỏ dấu ngoặc kép nếu có
                if ((filePath.StartsWith("\"") && filePath.EndsWith("\"")) ||
                    (filePath.StartsWith("'") && filePath.EndsWith("'")))
                {
                    filePath = filePath[1..^1];
                }
                
                if (!File.Exists(filePath))
                {
                    attempts++;
                    Console.WriteLine($"File không tồn tại: '{filePath}'. Vui lòng kiểm tra lại đường dẫn.");
                    continue;
                }
                
                return filePath;
            }
            
            throw new Exception($"Quá nhiều lần nhập sai đường dẫn file ({maxAttempts} lần). Chương trình sẽ thoát.");
        }

        public ComparisonType GetComparisonType()
        {
            Console.WriteLine("\nChọn loại so sánh:");
            Console.WriteLine("1. So sánh hai cột đơn (A vs B)");
            Console.WriteLine("2. So sánh nhóm cột (ID + Amount)");
            
            int attempts = 0;
            const int maxAttempts = 5;
            
            while (attempts < maxAttempts)
            {
                Console.Write("Nhập lựa chọn (1 hoặc 2): ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    attempts++;
                    Console.WriteLine("Input trống! Vui lòng nhập 1 hoặc 2.");
                    continue;
                }
                
                if (input.Trim() == "1")
                    return ComparisonType.TwoColumns;
                else if (input.Trim() == "2")
                    return ComparisonType.GroupColumns;
                else
                {
                    attempts++;
                    Console.WriteLine($"Input không hợp lệ: '{input}'. Vui lòng nhập 1 hoặc 2!");
                }
            }
            
            Console.WriteLine($"Quá nhiều lần nhập sai ({maxAttempts} lần). Sử dụng mặc định: So sánh hai cột đơn.");
            return ComparisonType.TwoColumns;
        }

        public Encoding? SelectEncoding()
        {
            Console.WriteLine("\n🔧 Auto-detect encoding không thành công.");
            Console.WriteLine("Vui lòng chọn encoding thủ công:");
            Console.WriteLine("1. UTF-8");
            Console.WriteLine("2. UTF-8 with BOM");
            Console.WriteLine("3. Windows-1258 (Vietnamese)");
            Console.WriteLine("4. ASCII");
            Console.WriteLine("5. Windows-1252 (Western European)");
            Console.WriteLine("6. Bỏ qua (sử dụng UTF-8)");
            
            int attempts = 0;
            const int maxAttempts = 3;
            
            while (attempts < maxAttempts)
            {
                Console.Write("Nhập lựa chọn (1-6): ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    attempts++;
                    continue;
                }
                
                switch (input.Trim())
                {
                    case "1": return Encoding.UTF8;
                    case "2": return new UTF8Encoding(true); // UTF-8 with BOM
                    case "3": return Encoding.GetEncoding(1258); // Windows-1258
                    case "4": return Encoding.ASCII;
                    case "5": return Encoding.GetEncoding(1252); // Windows-1252
                    case "6": return null; // Skip manual selection
                    default:
                        attempts++;
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        break;
                }
            }
            
            return null; // Use default
        }

        public (bool hasHeader, List<string> columns) AnalyzeCsvFile(string filePath)
        {
            return _csvReaderService.AnalyzeCsvFile(filePath);
        }

        public (string, string) SelectTwoColumns(List<string> availableColumns)
        {
            if (availableColumns.Any())
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
                Console.Write("Nhập tên cột thứ nhất: ");
                string? col1 = Console.ReadLine();
                Console.Write("Nhập tên cột thứ hai: ");
                string? col2 = Console.ReadLine();
                
                return (col1 ?? "Column_1", col2 ?? "Column_2");
            }
        }

        public (GroupColumnConfiguration, GroupColumnConfiguration) SelectGroupColumns(List<string> availableColumns)
        {
            Console.WriteLine("\n=== CẤU HÌNH NHÓM 1 ===");
            var group1 = ConfigureGroup(availableColumns, "Nhóm 1");
            
            Console.WriteLine("\n=== CẤU HÌNH NHÓM 2 ===");
            var group2 = ConfigureGroup(availableColumns, "Nhóm 2");
            
            return (group1, group2);
        }

        private GroupColumnConfiguration ConfigureGroup(List<string> availableColumns, string groupName)
        {
            var config = new GroupColumnConfiguration { GroupName = groupName };
            
            if (availableColumns.Any())
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
            int attempts = 0;
            const int maxAttempts = 5;
            
            while (attempts < maxAttempts)
            {
                Console.Write($"{prompt} (số thứ tự hoặc tên cột): ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    attempts++;
                    Console.WriteLine("Input trống! Vui lòng nhập lựa chọn.");
                    continue;
                }
                
                input = input.Trim();
                
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
                
                attempts++;
                Console.WriteLine($"Lựa chọn không hợp lệ: '{input}'. Vui lòng nhập số từ 1-{availableColumns.Count} hoặc tên cột.");
            }
            
            Console.WriteLine($"Quá nhiều lần nhập sai ({maxAttempts} lần). Sử dụng cột đầu tiên: {availableColumns[0]}");
            return availableColumns[0];
        }

        public bool AskToContinue()
        {
            Console.WriteLine("\nBạn có muốn thực hiện so sánh khác không?");
            Console.WriteLine("1. Có");
            Console.WriteLine("2. Không");
            
            int attempts = 0;
            const int maxAttempts = 3;
            
            while (attempts < maxAttempts)
            {
                Console.Write("Nhập lựa chọn (1 hoặc 2): ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    attempts++;
                    Console.WriteLine("Input trống! Vui lòng nhập 1 hoặc 2.");
                    continue;
                }
                
                if (input.Trim() == "1")
                    return true;
                else if (input.Trim() == "2")
                    return false;
                else
                {
                    attempts++;
                    Console.WriteLine($"Input không hợp lệ: '{input}'. Vui lòng nhập 1 hoặc 2!");
                }
            }
            
            Console.WriteLine($"Quá nhiều lần nhập sai ({maxAttempts} lần). Thoát chương trình.");
            return false;
        }
    }
}