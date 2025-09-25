using CsvCompareApp.Models;

namespace CsvCompareApp.Services
{
    public class ComparisonService
    {
        public ComparisonResult CompareTwoColumns(List<Dictionary<string, object>> records, string columnA, string columnB)
        {
            var result = new ComparisonResult();
            
            Console.WriteLine($"=== SO SÁNH HAI CỘT: {columnA} ↔ {columnB} ===\n");

            // So sánh từng dòng
            Console.WriteLine("1️⃣ Các dòng có giá trị khác nhau:");
            for (int i = 0; i < records.Count; i++)
            {
                var valueA = records[i].GetValueOrDefault(columnA, "").ToString();
                var valueB = records[i].GetValueOrDefault(columnB, "").ToString();
                
                if (valueA != valueB)
                {
                    string difference = $"   Dòng {i + 1}: {columnA}='{valueA}', {columnB}='{valueB}'";
                    Console.WriteLine(difference);
                    result.Differences.Add(difference);
                }
            }
            
            if (result.Differences.Count == 0)
            {
                Console.WriteLine("   Không có dòng nào khác nhau.");
            }

            // Đếm số lần xuất hiện của mỗi giá trị
            var countsInA = records
                .Select(r => r.GetValueOrDefault(columnA, "").ToString())
                .Where(a => !string.IsNullOrEmpty(a))
                .GroupBy(a => a!)
                .ToDictionary(g => g.Key, g => g.Count());

            var countsInB = records
                .Select(r => r.GetValueOrDefault(columnB, "").ToString())
                .Where(b => !string.IsNullOrEmpty(b))
                .GroupBy(b => b!)
                .ToDictionary(g => g.Key, g => g.Count());

            // So sánh số lần xuất hiện
            var allKeys = countsInA.Keys.Union(countsInB.Keys).ToList();
            
            Console.WriteLine("\n2️⃣ Số lần xuất hiện khác nhau:");
            foreach (var key in allKeys)
            {
                int countA = countsInA.GetValueOrDefault(key, 0);
                int countB = countsInB.GetValueOrDefault(key, 0);
                
                if (countA != countB)
                {
                    string mismatch = $"   📊 Giá trị '{key}': {columnA}: {countA} lần, {columnB}: {countB} lần";
                    Console.WriteLine(mismatch);
                    result.AmountMismatches.Add(mismatch);
                }
            }

            // Tìm giá trị chỉ có trong A hoặc B
            var onlyInA = countsInA.Keys.Where(k => !countsInB.ContainsKey(k));
            var onlyInB = countsInB.Keys.Where(k => !countsInA.ContainsKey(k));

            Console.WriteLine($"\n3️⃣ Chỉ có trong {columnA}:");
            foreach (var a in onlyInA)
            {
                string onlyA = $"   ➡️ {a} ({countsInA[a]} lần)";
                Console.WriteLine(onlyA);
                result.OnlyInFirst.Add(onlyA);
            }
            
            if (!onlyInA.Any())
            {
                Console.WriteLine($"   ✅ Không có giá trị nào chỉ có trong {columnA}.");
            }

            Console.WriteLine($"\n4️⃣ Chỉ có trong {columnB}:");
            foreach (var b in onlyInB)
            {
                string onlyB = $"   {b} ({countsInB[b]} lần)";
                Console.WriteLine(onlyB);
                result.OnlyInSecond.Add(onlyB);
            }
            
            if (!onlyInB.Any())
            {
                Console.WriteLine($"   Không có giá trị nào chỉ có trong {columnB}.");
            }

            // Tóm tắt
            result.TotalFirstGroup = countsInA.Values.Sum();
            result.TotalSecondGroup = countsInB.Values.Sum();
            
            return result;
        }

        public ComparisonResult CompareGroupColumns(List<Dictionary<string, object>> records, 
            GroupColumnConfiguration groupA, GroupColumnConfiguration groupB)
        {
            var result = new ComparisonResult();
            
            Console.WriteLine($"=== 🔍 ĐỐI CHIẾU DỮ LIỆU: {groupA.GroupName} ↔ {groupB.GroupName} ===\n");

            // Tạo dictionary cho mỗi nhóm với ID dưới dạng số nguyên
            var dictA = CreateGroupDictionary(records, groupA);
            var dictB = CreateGroupDictionary(records, groupB);

            result.TotalFirstGroup = dictA.Count;
            result.TotalSecondGroup = dictB.Count;

            Console.WriteLine($"📊 Đã tải: {dictA.Count} dòng từ {groupA.GroupName}, {dictB.Count} dòng từ {groupB.GroupName}\n");

            // 1. So sánh ID khớp nhưng Amount khác
            Console.WriteLine($"1️⃣ 💰 CÁC ID KHỚP NHƯNG SỐ TIỀN KHÁC NHAU:");
            foreach (var kvp in dictA)
            {
                long id = kvp.Key;
                decimal amountA = kvp.Value;
                
                if (dictB.ContainsKey(id))
                {
                    decimal amountB = dictB[id];
                    if (amountA != amountB)
                    {
                        var difference = amountA - amountB;
                        string sign = difference > 0 ? "+" : "";
                        string mismatch = $"   🆔 ID: {id:D6} | {groupA.GroupName}: {amountA:N0}đ | {groupB.GroupName}: {amountB:N0}đ | 📈 Chênh: {sign}{difference:N0}đ";
                        Console.WriteLine(mismatch);
                        result.AmountMismatches.Add(mismatch);
                    }
                }
            }
            
            if (result.AmountMismatches.Count == 0)
            {
                Console.WriteLine("   ✅ Tuyệt vời! Không có ID nào lệch số tiền.");
            }

            // 2. ID chỉ có ở nhóm A
            Console.WriteLine($"\n2️⃣ ➡️ CÁC ID CHỈ CÓ TRONG {groupA.GroupName}:");
            var onlyInA = dictA.Keys.Where(id => !dictB.ContainsKey(id)).ToList();
            foreach (var id in onlyInA)
            {
                string onlyA = $"   🔹 ID: {id:D6} | Số tiền: {dictA[id]:N0}đ";
                Console.WriteLine(onlyA);
                result.OnlyInFirst.Add(onlyA);
            }
            
            if (onlyInA.Count == 0)
            {
                Console.WriteLine($"   ✅ Không có ID nào chỉ có trong {groupA.GroupName}.");
            }

            // 3. ID chỉ có ở nhóm B
            Console.WriteLine($"\n3️⃣ ⬅️ CÁC ID CHỈ CÓ TRONG {groupB.GroupName}:");
            var onlyInB = dictB.Keys.Where(id => !dictA.ContainsKey(id)).ToList();
            foreach (var id in onlyInB)
            {
                string onlyB = $"   🔸 ID: {id:D6} | Số tiền: {dictB[id]:N0}đ";
                Console.WriteLine(onlyB);
                result.OnlyInSecond.Add(onlyB);
            }
            
            if (onlyInB.Count == 0)
            {
                Console.WriteLine($"   ✅ Không có ID nào chỉ có trong {groupB.GroupName}.");
            }

            // Tính số khớp hoàn toàn
            result.PerfectMatches = dictA.Keys.Count(id => dictB.ContainsKey(id) && dictA[id] == dictB[id]);

            // Tổng kết với emoji và định dạng đẹp hơn
            Console.WriteLine($"\n" + new string('=', 55));
            Console.WriteLine($"📋 TỔNG KẾT ĐỐI CHIẾU");
            Console.WriteLine($"" + new string('=', 55));
            Console.WriteLine($"📊 Tổng ID trong {groupA.GroupName}: {result.TotalFirstGroup:N0}");
            Console.WriteLine($"📊 Tổng ID trong {groupB.GroupName}: {result.TotalSecondGroup:N0}");
            Console.WriteLine($"✅ ID khớp hoàn toàn: {result.PerfectMatches:N0}");
            Console.WriteLine($"💰 ID lệch số tiền: {result.AmountMismatches.Count:N0}");
            Console.WriteLine($"➡️ ID chỉ có trong {groupA.GroupName}: {result.OnlyInFirst.Count:N0}");
            Console.WriteLine($"⬅️ ID chỉ có trong {groupB.GroupName}: {result.OnlyInSecond.Count:N0}");
            
            if (result.AmountMismatches.Count == 0 && result.OnlyInFirst.Count == 0 && result.OnlyInSecond.Count == 0)
            {
                Console.WriteLine($"\n🎉 HOÀN HẢO! Tất cả dữ liệu đều khớp nhau!");
            }
            else
            {
                var totalIssues = result.AmountMismatches.Count + result.OnlyInFirst.Count + result.OnlyInSecond.Count;
                Console.WriteLine($"\n⚠️ Phát hiện {totalIssues:N0} điểm khác biệt cần kiểm tra");
            }
            Console.WriteLine("" + new string('=', 55) + "\n");

            return result;
        }

        private Dictionary<long, decimal> CreateGroupDictionary(List<Dictionary<string, object>> records, 
            GroupColumnConfiguration config)
        {
            var dictionary = new Dictionary<long, decimal>();
            
            foreach (var record in records)
            {
                var idValue = record.GetValueOrDefault(config.IdColumn, "").ToString();
                var amountValue = record.GetValueOrDefault(config.AmountColumn, "0").ToString();
                
                if (!string.IsNullOrEmpty(idValue) && TryParseId(idValue, out long id))
                {
                    // Parse amount - loại bỏ dấu phẩy và khoảng trắng
                    var cleanAmount = amountValue?.Replace(",", "").Replace(" ", "").Trim() ?? "0";
                    if (decimal.TryParse(cleanAmount, out decimal amount))
                    {
                        dictionary[id] = amount;
                    }
                }
            }
            
            return dictionary;
        }

        private bool TryParseId(string idString, out long id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(idString))
                return false;

            // Loại bỏ khoảng trắng và các ký tự không cần thiết
            var cleanId = idString.Trim().Replace(" ", "");
            
            // Thử parse trực tiếp
            if (long.TryParse(cleanId, out id))
                return true;
            
            // Nếu có số 0 đầu hoặc ký tự đặc biệt, extract số
            var numberOnly = new string(cleanId.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(numberOnly) && long.TryParse(numberOnly, out id))
                return true;
            
            return false;
        }

        public void PrintSummary(ComparisonResult result, ComparisonType comparisonType)
        {
            Console.WriteLine("\n=== TÓM TẮT ===");
            
            if (comparisonType == ComparisonType.TwoColumns)
            {
                Console.WriteLine($"Tổng số giá trị cột 1: {result.TotalFirstGroup}");
                Console.WriteLine($"Tổng số giá trị cột 2: {result.TotalSecondGroup}");
                Console.WriteLine($"Số dòng khác nhau: {result.Differences.Count}");
                Console.WriteLine($"Giá trị chỉ có ở cột 1: {result.OnlyInFirst.Count}");
                Console.WriteLine($"Giá trị chỉ có ở cột 2: {result.OnlyInSecond.Count}");
                Console.WriteLine($"Giá trị có số lần xuất hiện khác nhau: {result.AmountMismatches.Count}");
            }
            else
            {
                Console.WriteLine($"Tổng số ID nhóm 1: {result.TotalFirstGroup}");
                Console.WriteLine($"Tổng số ID nhóm 2: {result.TotalSecondGroup}");
                Console.WriteLine($"ID khớp nhưng lệch Amount: {result.AmountMismatches.Count}");
                Console.WriteLine($"ID chỉ có ở nhóm 1: {result.OnlyInFirst.Count}");
                Console.WriteLine($"ID chỉ có ở nhóm 2: {result.OnlyInSecond.Count}");
                Console.WriteLine($"ID khớp hoàn toàn (ID và Amount): {result.PerfectMatches}");
            }
        }
    }
}