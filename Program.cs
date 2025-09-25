using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

class Program
{
    static void Main(string[] args)
    {
        var filePath = @"D:\FPT\projects\CsvCompareApp\Book3.csv"; // đường dẫn tuyệt đối đến file CSV
        using var reader = new StreamReader(filePath);
        
        // Cấu hình để đọc file CSV với header
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };
        using var csv = new CsvReader(reader, config);
        
        var records = csv.GetRecords<InvoiceData>().ToList();

        // Tạo dictionary cho Nhóm A và Nhóm B để so sánh theo ID
        var groupA = records
            .Where(r => !string.IsNullOrEmpty(r.ID_A))
            .ToDictionary(r => r.ID_A!, r => r.Amount_A);

        var groupB = records
            .Where(r => !string.IsNullOrEmpty(r.ID_B))
            .ToDictionary(r => r.ID_B!, r => r.Amount_B);

        Console.WriteLine("=== KẾT QUẢ SO SÁNH HÓA ĐƠN ===\n");

        // 1. So sánh hóa đơn có cùng ID nhưng khác số tiền
        Console.WriteLine("1. Hóa đơn khớp ID nhưng lệch số tiền:");
        var amountMismatches = new List<string>();
        foreach (var kvp in groupA)
        {
            string id = kvp.Key;
            decimal amountA = kvp.Value;
            
            if (groupB.ContainsKey(id))
            {
                decimal amountB = groupB[id];
                if (amountA != amountB)
                {
                    string message = $"   ID: {id} | Amount_A: {amountA:C} | Amount_B: {amountB:C} | Chênh lệch: {amountA - amountB:C}";
                    Console.WriteLine(message);
                    amountMismatches.Add(message);
                }
            }
        }
        if (amountMismatches.Count == 0)
        {
            Console.WriteLine("   Không có hóa đơn nào lệch số tiền.");
        }

        // 2. Hóa đơn chỉ có ở Nhóm A (thiếu ở B)
        Console.WriteLine("\n2. Hóa đơn chỉ có ở Nhóm A (thiếu ở Nhóm B):");
        var onlyInA = groupA.Keys.Where(id => !groupB.ContainsKey(id)).ToList();
        if (onlyInA.Count > 0)
        {
            foreach (var id in onlyInA)
            {
                Console.WriteLine($"   ID: {id} | Amount: {groupA[id]:C}");
            }
        }
        else
        {
            Console.WriteLine("   Không có hóa đơn nào chỉ có ở Nhóm A.");
        }

        // 3. Hóa đơn chỉ có ở Nhóm B (thiếu ở A)
        Console.WriteLine("\n3. Hóa đơn chỉ có ở Nhóm B (thiếu ở Nhóm A):");
        var onlyInB = groupB.Keys.Where(id => !groupA.ContainsKey(id)).ToList();
        if (onlyInB.Count > 0)
        {
            foreach (var id in onlyInB)
            {
                Console.WriteLine($"   ID: {id} | Amount: {groupB[id]:C}");
            }
        }
        else
        {
            Console.WriteLine("   Không có hóa đơn nào chỉ có ở Nhóm B.");
        }

        // 4. Tóm tắt
        Console.WriteLine("\n=== TÓM TẮT ===");
        Console.WriteLine($"Tổng số hóa đơn Nhóm A: {groupA.Count}");
        Console.WriteLine($"Tổng số hóa đơn Nhóm B: {groupB.Count}");
        Console.WriteLine($"Hóa đơn khớp ID nhưng lệch tiền: {amountMismatches.Count}");
        Console.WriteLine($"Hóa đơn chỉ có ở A: {onlyInA.Count}");
        Console.WriteLine($"Hóa đơn chỉ có ở B: {onlyInB.Count}");
        
        // Tính số hóa đơn khớp hoàn toàn
        var perfectMatches = groupA.Keys.Where(id => groupB.ContainsKey(id) && groupA[id] == groupB[id]).Count();
        Console.WriteLine($"Hóa đơn khớp hoàn toàn (ID và Amount): {perfectMatches}");
    }

    public class InvoiceData
    {
        [CsvHelper.Configuration.Attributes.Name("ID_A")]
        public string? ID_A { get; set; }
        
        [CsvHelper.Configuration.Attributes.Name("Amount_A")]
        public decimal Amount_A { get; set; }
        
        [CsvHelper.Configuration.Attributes.Name("ID_B")]
        public string? ID_B { get; set; }
        
        [CsvHelper.Configuration.Attributes.Name("Amount_B")]
        public decimal Amount_B { get; set; }
    }
}
