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
            
            Console.WriteLine($"=== SO SÁNH NHÓM CỘT: {groupA.GroupName} vs {groupB.GroupName} ===\n");

            // Tạo dictionary cho mỗi nhóm
            var dictA = CreateGroupDictionary(records, groupA);
            var dictB = CreateGroupDictionary(records, groupB);

            result.TotalFirstGroup = dictA.Count;
            result.TotalSecondGroup = dictB.Count;

            // 1. So sánh ID khớp nhưng Amount khác
            Console.WriteLine($"1. {groupA.GroupName} và {groupB.GroupName} khớp ID nhưng lệch Amount:");
            foreach (var kvp in dictA)
            {
                string id = kvp.Key;
                decimal amountA = kvp.Value;
                
                if (dictB.ContainsKey(id))
                {
                    decimal amountB = dictB[id];
                    if (amountA != amountB)
                    {
                        string mismatch = $"   ID: {id} | {groupA.GroupName}: {amountA:C} | {groupB.GroupName}: {amountB:C} | Chênh lệch: {amountA - amountB:C}";
                        Console.WriteLine(mismatch);
                        result.AmountMismatches.Add(mismatch);
                    }
                }
            }
            
            if (result.AmountMismatches.Count == 0)
            {
                Console.WriteLine("   Không có ID nào lệch Amount.");
            }

            // 2. ID chỉ có ở nhóm A
            Console.WriteLine($"\n2. ID chỉ có ở {groupA.GroupName}:");
            var onlyInA = dictA.Keys.Where(id => !dictB.ContainsKey(id)).ToList();
            foreach (var id in onlyInA)
            {
                string onlyA = $"   ID: {id} | Amount: {dictA[id]:C}";
                Console.WriteLine(onlyA);
                result.OnlyInFirst.Add(onlyA);
            }
            
            if (onlyInA.Count == 0)
            {
                Console.WriteLine($"   Không có ID nào chỉ có ở {groupA.GroupName}.");
            }

            // 3. ID chỉ có ở nhóm B
            Console.WriteLine($"\n3. ID chỉ có ở {groupB.GroupName}:");
            var onlyInB = dictB.Keys.Where(id => !dictA.ContainsKey(id)).ToList();
            foreach (var id in onlyInB)
            {
                string onlyB = $"   ID: {id} | Amount: {dictB[id]:C}";
                Console.WriteLine(onlyB);
                result.OnlyInSecond.Add(onlyB);
            }
            
            if (onlyInB.Count == 0)
            {
                Console.WriteLine($"   Không có ID nào chỉ có ở {groupB.GroupName}.");
            }

            // Tính số khớp hoàn toàn
            result.PerfectMatches = dictA.Keys.Count(id => dictB.ContainsKey(id) && dictA[id] == dictB[id]);

            return result;
        }

        private Dictionary<string, decimal> CreateGroupDictionary(List<Dictionary<string, object>> records, 
            GroupColumnConfiguration config)
        {
            var dictionary = new Dictionary<string, decimal>();
            
            foreach (var record in records)
            {
                var idValue = record.GetValueOrDefault(config.IdColumn, "").ToString();
                var amountValue = record.GetValueOrDefault(config.AmountColumn, "0").ToString();
                
                if (!string.IsNullOrEmpty(idValue) && decimal.TryParse(amountValue, out decimal amount))
                {
                    dictionary[idValue] = amount;
                }
            }
            
            return dictionary;
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