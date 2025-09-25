using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvCompareApp.Models;
using ClosedXML.Excel;

namespace CsvCompareApp.Services
{
    public class CsvReaderService
    {
        public List<Dictionary<string, object>> ReadCsvFile(ColumnConfiguration config)
        {
            // Kiểm tra extension của file để quyết định phương thức đọc
            var extension = Path.GetExtension(config.FilePath).ToLowerInvariant();
            
            if (extension == ".xlsx" || extension == ".xls")
            {
                return ReadExcelFile(config);
            }
            else
            {
                return ReadCsvFileWithEncoding(config, null);
            }
        }

        public List<Dictionary<string, object>> ReadExcelFile(ColumnConfiguration config)
        {
            try
            {
                Console.WriteLine($"📊 Đang đọc file Excel: {config.FilePath}");
                
                using var workbook = new XLWorkbook(config.FilePath);
                var worksheet = workbook.Worksheet(1); // Đọc sheet đầu tiên
                
                Console.WriteLine($"📋 Sheet: {worksheet.Name}");
                
                var records = new List<Dictionary<string, object>>();
                var rows = worksheet.RowsUsed().ToList();
                
                if (rows.Count == 0)
                {
                    Console.WriteLine("⚠️ File Excel trống!");
                    return records;
                }

                var firstRow = rows[0];
                var totalColumns = firstRow.CellsUsed().Count();
                Console.WriteLine($"📊 Phát hiện {totalColumns} cột trong file Excel");
                
                int startRowIndex = config.HasHeaderRecord ? 1 : 0; // Bắt đầu từ dòng 1 nếu có header, ngược lại từ dòng 0
                
                // Lấy danh sách column names từ config hoặc tạo tự động
                var columnNames = config.ColumnNames;
                if (columnNames.Count < totalColumns)
                {
                    // Nếu config không đủ cột, tạo thêm cột
                    var additionalColumns = GenerateColumnNames(totalColumns);
                    columnNames = additionalColumns.Take(totalColumns).ToList();
                    Console.WriteLine($"🔧 Đã tạo thêm tên cột: [{string.Join(", ", columnNames)}]");
                }
                
                // Handle duplicate column names by using column indexes
                var uniqueColumnKeys = new List<string>();
                for (int i = 0; i < Math.Min(columnNames.Count, totalColumns); i++)
                {
                    uniqueColumnKeys.Add($"{columnNames[i]}#{i+1}");
                }
                
                // Đọc dữ liệu từ các dòng
                for (int rowIndex = startRowIndex; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    var record = new Dictionary<string, object>();
                    
                    var cells = row.CellsUsed().ToList();
                    
                    // Đọc từng cột
                    for (int colIndex = 0; colIndex < totalColumns && colIndex < columnNames.Count; colIndex++)
                    {
                        string value = "";
                        
                        // Tìm cell tương ứng với column index (Excel có thể có gap)
                        var cell = cells.FirstOrDefault(c => c.Address.ColumnNumber == colIndex + 1);
                        if (cell != null)
                        {
                            value = cell.GetValue<string>() ?? "";
                        }
                        
                        // Store with unique key to avoid duplicate key issues
                        record[uniqueColumnKeys[colIndex]] = value;
                    }
                    
                    records.Add(record);
                }
                
                Console.WriteLine($"✅ Đã đọc {records.Count} dòng từ file Excel");
                return records;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi đọc file Excel: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }
        
        public List<Dictionary<string, object>> ReadCsvFileWithEncoding(ColumnConfiguration config, Encoding? forcedEncoding)
        {
            try
            {
                // Sử dụng encoding được chỉ định hoặc auto-detect
                var encoding = forcedEncoding ?? DetectFileEncoding(config.FilePath);
                Console.WriteLine($"📄 Using encoding: {encoding.EncodingName}");
                
                using var stream = new FileStream(config.FilePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream, encoding);
                
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = config.HasHeaderRecord,
                    Delimiter = ",", // Explicitly set delimiter
                    Quote = '"',
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null, // Ignore missing fields
                    HeaderValidated = null, // Ignore header validation
                    BadDataFound = null // Ignore bad data
                };
                
                using var csv = new CsvReader(reader, csvConfig);
                
                var records = new List<Dictionary<string, object>>();
                
                if (config.HasHeaderRecord)
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    // Use unique headers for reading
                    var uniqueHeaders = GetUniqueHeaders(csv.HeaderRecord);
                    
                    while (csv.Read())
                    {
                        var record = new Dictionary<string, object>();
                        for (int i = 0; i < uniqueHeaders.Count; i++)
                        {
                            try
                            {
                                var value = csv.GetField(i);
                                record[uniqueHeaders[i]] = value ?? "";
                            }
                            catch
                            {
                                record[uniqueHeaders[i]] = "";
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

        public (bool hasHeader, List<string> columns) AnalyzeCsvFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (extension == ".xlsx" || extension == ".xls")
            {
                return AnalyzeExcelFile(filePath);
            }
            else
            {
                return AnalyzeCsvFileInternal(filePath);
            }
        }

        public (bool hasHeader, List<string> columns) AnalyzeExcelFile(string filePath)
        {
            try
            {
                Console.WriteLine($"🔍 Đang phân tích file Excel: {filePath}");
                
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                
                var rows = worksheet.RowsUsed().ToList();
                if (rows.Count == 0)
                {
                    return (false, new List<string>());
                }

                var firstRow = rows[0];
                var totalColumns = firstRow.CellsUsed().Count();
                
                Console.WriteLine($"📊 Phát hiện {totalColumns} cột trong Excel");
                
                // Đọc dòng đầu tiên
                var firstRowValues = new List<string>();
                for (int i = 1; i <= totalColumns; i++)
                {
                    var cellValue = firstRow.Cell(i).GetValue<string>() ?? "";
                    firstRowValues.Add(cellValue);
                }
                
                // Nếu chỉ có 1 dòng, coi như không có header
                if (rows.Count < 2)
                {
                    var columnNames = GenerateColumnNames(totalColumns);
                    Console.WriteLine($"📊 Chỉ có 1 dòng, tạo tên cột tự động: [{string.Join(", ", columnNames)}]");
                    return (false, columnNames);
                }
                
                // Đọc dòng thứ hai
                var secondRow = rows[1];
                var secondRowValues = new List<string>();
                for (int i = 1; i <= totalColumns; i++)
                {
                    var cellValue = secondRow.Cell(i).GetValue<string>() ?? "";
                    secondRowValues.Add(cellValue);
                }
                
                // Phân tích xem dòng đầu có phải header không
                bool hasHeader = DetectHeader(firstRowValues, secondRowValues);
                
                if (hasHeader)
                {
                    Console.WriteLine($"✅ Phát hiện header trong Excel: [{string.Join(", ", firstRowValues)}]");
                    return (true, firstRowValues);
                }
                else
                {
                    var columnNames = GenerateColumnNames(totalColumns);
                    Console.WriteLine($"📊 Không có header, tạo tên cột tự động: [{string.Join(", ", columnNames)}]");
                    return (false, columnNames);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi phân tích file Excel: {ex.Message}");
                return (false, new List<string>());
            }
        }

        private (bool hasHeader, List<string> columns) AnalyzeCsvFileInternal(string filePath)
        {
            try
            {
                var encoding = DetectFileEncoding(filePath);
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream, encoding);
                
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    Quote = '"',
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    BadDataFound = null
                };
                
                using var csv = new CsvReader(reader, csvConfig);
                
                // Đọc dòng đầu tiên
                if (!csv.Read()) 
                    return (false, new List<string>());
                
                var firstRow = new List<string>();
                for (int i = 0; i < csv.ColumnCount; i++)
                {
                    firstRow.Add(csv.GetField(i) ?? "");
                }
                
                // Đọc dòng thứ hai để phân tích
                if (!csv.Read())
                {
                    // Chỉ có 1 dòng, coi như không có header
                    return (false, GenerateColumnNames(firstRow.Count));
                }
                
                var secondRow = new List<string>();
                for (int i = 0; i < csv.ColumnCount; i++)
                {
                    secondRow.Add(csv.GetField(i) ?? "");
                }
                
                // Phân tích xem dòng đầu có phải header không
                bool hasHeader = DetectHeader(firstRow, secondRow);
                
                if (hasHeader)
                {
                    var uniqueHeaders = GetUniqueHeaders(firstRow);
                    Console.WriteLine($"✅ Phát hiện header: [{string.Join(", ", uniqueHeaders)}]");
                    return (true, uniqueHeaders);
                }
                else
                {
                    var columnNames = GenerateColumnNames(firstRow.Count);
                    Console.WriteLine($"📊 Không có header, tạo tên cột tự động: [{string.Join(", ", columnNames)}]");
                    return (false, columnNames);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi phân tích file CSV: {ex.Message}");
                return (false, new List<string>());
            }
        }

        private bool DetectHeader(List<string> firstRow, List<string> secondRow)
        {
            int numericInFirst = 0;
            int numericInSecond = 0;
            
            for (int i = 0; i < Math.Min(firstRow.Count, secondRow.Count); i++)
            {
                // Kiểm tra xem có phải số không (bao gồm cả decimal và integer)
                if (IsNumeric(firstRow[i]))
                    numericInFirst++;
                    
                if (IsNumeric(secondRow[i]))
                    numericInSecond++;
            }
            
            // Nếu dòng đầu có ít số hơn dòng thứ hai đáng kể, có thể là header
            // Hoặc nếu dòng đầu không có số nào nhưng dòng thứ hai có số
            return (numericInFirst < numericInSecond) || 
                   (numericInFirst == 0 && numericInSecond > 0) ||
                   (firstRow.Any(cell => ContainsLetters(cell)) && secondRow.All(cell => IsNumeric(cell) || string.IsNullOrEmpty(cell)));
        }

        private bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            return decimal.TryParse(value.Replace(",", ""), out _) || 
                   double.TryParse(value.Replace(",", ""), out _) ||
                   int.TryParse(value.Replace(",", ""), out _);
        }

        private bool ContainsLetters(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Any(char.IsLetter);
        }

        private List<string> GenerateColumnNames(int columnCount)
        {
            var columns = new List<string>();
            for (int i = 0; i < columnCount; i++)
            {
                columns.Add($"Column_{i + 1}");
            }
            return columns;
        }

        public List<string> GetCsvHeaders(string filePath)
        {
            var (hasHeader, columns) = AnalyzeCsvFile(filePath);
            return columns;
        }

        public List<string> GetFileHeaders(string filePath)
        {
            // Method tổng quát cho cả CSV và Excel
            return GetCsvHeaders(filePath);
        }

        private Encoding DetectFileEncoding(string filePath)
        {
            try
            {
                // Đọc toàn bộ file để analyze tốt hơn
                byte[] buffer = File.ReadAllBytes(filePath);
                
                // Check for BOM (Byte Order Mark)
                if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    Console.WriteLine("🔍 Found UTF-8 BOM");
                    return Encoding.UTF8;
                }
                
                if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    Console.WriteLine("🔍 Found UTF-16 LE BOM");
                    return Encoding.Unicode;
                }
                
                if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    Console.WriteLine("🔍 Found UTF-16 BE BOM");
                    return Encoding.BigEndianUnicode;
                }
                
                // Thử các encoding phổ biến cho tiếng Việt
                var encodingsToTry = new[]
                {
                    Encoding.UTF8,
                    Encoding.GetEncoding("Windows-1258"), // Vietnamese
                    Encoding.GetEncoding("UTF-8"),
                    Encoding.ASCII,
                    Encoding.Default
                };
                
                foreach (var encoding in encodingsToTry)
                {
                    try
                    {
                        string decoded = encoding.GetString(buffer);
                        
                        // Check nếu decode thành công và có ký tự tiếng Việt
                        if (IsValidVietnameseText(decoded))
                        {
                            Console.WriteLine($"🔍 Successfully decoded with {encoding.EncodingName}");
                            return encoding;
                        }
                        
                        // Check nếu không có ký tự lỗi
                        if (!decoded.Contains("�") && !decoded.Contains("?"))
                        {
                            Console.WriteLine($"🔍 Clean decode with {encoding.EncodingName}");
                            return encoding;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                // Default fallback
                Console.WriteLine("🔍 Using UTF-8 as fallback");
                return Encoding.UTF8;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error detecting encoding: {ex.Message}. Using UTF-8.");
                return Encoding.UTF8;
            }
        }

        private bool IsValidVietnameseText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            // Check for Vietnamese characters
            var vietnameseChars = new[]
            {
                'à', 'á', 'ạ', 'ả', 'ã', 'â', 'ầ', 'ấ', 'ậ', 'ẩ', 'ẫ', 'ă', 'ằ', 'ắ', 'ặ', 'ẳ', 'ẵ',
                'è', 'é', 'ẹ', 'ẻ', 'ẽ', 'ê', 'ề', 'ế', 'ệ', 'ể', 'ễ',
                'ì', 'í', 'ị', 'ỉ', 'ĩ',
                'ò', 'ó', 'ọ', 'ỏ', 'õ', 'ô', 'ồ', 'ố', 'ộ', 'ổ', 'ỗ', 'ơ', 'ờ', 'ớ', 'ợ', 'ở', 'ỡ',
                'ù', 'ú', 'ụ', 'ủ', 'ũ', 'ư', 'ừ', 'ứ', 'ự', 'ử', 'ữ',
                'ỳ', 'ý', 'ỵ', 'ỷ', 'ỹ',
                'đ',
                'À', 'Á', 'Ạ', 'Ả', 'Ã', 'Â', 'Ầ', 'Ấ', 'Ậ', 'Ẩ', 'Ẫ', 'Ă', 'Ằ', 'Ắ', 'Ặ', 'Ẳ', 'Ẵ',
                'È', 'É', 'Ẹ', 'Ẻ', 'Ẽ', 'Ê', 'Ề', 'Ế', 'Ệ', 'Ể', 'Ễ',
                'Ì', 'Í', 'Ị', 'Ỉ', 'Ĩ',
                'Ò', 'Ó', 'Ọ', 'Ỏ', 'Õ', 'Ô', 'Ồ', 'Ố', 'Ộ', 'Ổ', 'Ỗ', 'Ơ', 'Ờ', 'Ớ', 'Ợ', 'Ở', 'Ỡ',
                'Ù', 'Ú', 'Ụ', 'Ủ', 'Ũ', 'Ư', 'Ừ', 'Ứ', 'Ự', 'Ử', 'Ữ',
                'Ỳ', 'Ý', 'Ỵ', 'Ỷ', 'Ỹ',
                'Đ'
            };
            
            // Check nếu text chứa ký tự tiếng Việt và không có ký tự lỗi
            bool hasVietnamese = vietnameseChars.Any(c => text.Contains(c));
            bool hasErrorChars = text.Contains('�') || text.Contains('?');
            
            return hasVietnamese && !hasErrorChars;
        }

        private bool IsValidUtf8(byte[] buffer, int length)
        {
            try
            {
                var decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[length];
                decoder.Convert(buffer, 0, length, chars, 0, chars.Length, false, out int bytesUsed, out int charsUsed, out bool completed);
                return completed && bytesUsed == length;
            }
            catch
            {
                return false;
            }
        }

        private bool ContainsVietnameseCharacters(byte[] buffer, int length)
        {
            // Check for Vietnamese specific byte patterns in Windows-1258
            for (int i = 0; i < length; i++)
            {
                byte b = buffer[i];
                // Vietnamese characters in Windows-1258: À, Á, È, É, Ì, Í, Ò, Ó, Ù, Ú, Ý, etc.
                if (b >= 0xC0 && b <= 0xFF)
                {
                    return true;
                }
            }
            return false;
        }

        private List<string> GetUniqueHeaders(IEnumerable<string> headers)
        {
            var uniqueHeaders = new List<string>();
            var counts = new Dictionary<string, int>();
            
            foreach (var header in headers)
            {
                if (counts.ContainsKey(header))
                {
                    counts[header]++;
                    uniqueHeaders.Add($"{header}_{counts[header]}");
                }
                else
                {
                    counts[header] = 1;
                    uniqueHeaders.Add(header);
                }
            }
            return uniqueHeaders;
        }
    }
}