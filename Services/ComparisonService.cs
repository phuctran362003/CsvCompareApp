using CsvCompareApp.Models;

namespace CsvCompareApp.Services
{
    public class ComparisonService
    {
        public GroupComparisonResult CompareGroupColumns(List<Dictionary<string, object>> records, GroupColumnConfiguration config)
        {
            Console.WriteLine($"\n--- ĐỐI CHIẾU DỮ LIỆU NHÓM ---");

            // Create dictionaries for each group using the correct unique column keys
            var dictA = CreateGroupDictionary(records, config.GroupAIdColumn, config.GroupAAmountColumn);
            var dictB = CreateGroupDictionary(records, config.GroupBIdColumn, config.GroupBAmountColumn);

            Console.WriteLine($"Đã tải: {dictA.Count} mục từ Nhóm A, {dictB.Count} mục từ Nhóm B\n");

            var result = new GroupComparisonResult
            {
                TotalInGroupA = dictA.Count,
                TotalInGroupB = dictB.Count
            };

            var allIds = dictA.Keys.Union(dictB.Keys).ToHashSet();

            foreach (var id in allIds)
            {
                bool inA = dictA.TryGetValue(id, out var amountA);
                bool inB = dictB.TryGetValue(id, out var amountB);

                if (inA && inB)
                {
                    if (amountA == amountB)
                    {
                        result.Matches.Add(new MatchedItem { Id = id, Amount = amountA });
                    }
                    else
                    {
                        result.Mismatches.Add(new MismatchedItem { Id = id, AmountA = amountA, AmountB = amountB });
                    }
                }
                else if (inA)
                {
                    result.OnlyInA.Add(new GroupItem { Id = id, Amount = amountA });
                }
                else
                {
                    result.OnlyInB.Add(new GroupItem { Id = id, Amount = amountB });
                }
            }
            
            return result;
        }

        public TwoColumnComparisonResult CompareTwoColumns(List<Dictionary<string, object>> records, string columnA, string columnB)
        {
            var result = new TwoColumnComparisonResult();
            
            Console.WriteLine($"=== SO SÁNH HAI CỘT: {columnA} vs {columnB} ===\n");

            // So sánh từng dòng
            Console.WriteLine("Các dòng có giá trị khác nhau:");
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
            
            Console.WriteLine("\nSố lần xuất hiện khác nhau:");
            foreach (var key in allKeys)
            {
                int countA = countsInA.GetValueOrDefault(key, 0);
                int countB = countsInB.GetValueOrDefault(key, 0);
                
                if (countA != countB)
                {
                    string mismatch = $"   Giá trị '{key}': {columnA}: {countA} lần, {columnB}: {countB} lần";
                    Console.WriteLine(mismatch);
                    result.AmountMismatches.Add(mismatch);
                }
            }

            // Tìm giá trị chỉ có trong A hoặc B
            var onlyInA = countsInA.Keys.Where(k => !countsInB.ContainsKey(k));
            var onlyInB = countsInB.Keys.Where(k => !countsInA.ContainsKey(k));

            Console.WriteLine($"\nChỉ có trong {columnA}:");
            foreach (var a in onlyInA)
            {
                string onlyA = $"   {a} ({countsInA[a]} lần)";
                Console.WriteLine(onlyA);
                result.OnlyInFirst.Add(onlyA);
            }
            
            if (!onlyInA.Any())
            {
                Console.WriteLine($"   Không có giá trị nào chỉ có trong {columnA}.");
            }

            Console.WriteLine($"\nChỉ có trong {columnB}:");
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

        private Dictionary<long, decimal> CreateGroupDictionary(List<Dictionary<string, object>> records, 
            string idColumnName, string amountColumnName)
        {
            var dictionary = new Dictionary<long, decimal>();
            
            foreach (var record in records)
            {
                var idValue = record.GetValueOrDefault(idColumnName, "").ToString();
                var amountValue = record.GetValueOrDefault(amountColumnName, "0").ToString();
                
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

        public void PrintGroupSummary(GroupComparisonResult result)
        {
            Console.WriteLine("\n--- KẾT QUẢ SO SÁNH NHÓM ---");
            Console.WriteLine($"\n--- TỔNG QUAN ---");
            Console.WriteLine($"- {result.TotalInGroupA} mục trong Nhóm A");
            Console.WriteLine($"- {result.TotalInGroupB} mục trong Nhóm B");
            Console.WriteLine($"- {result.Matches.Count} mục trùng khớp hoàn toàn");
            Console.WriteLine($"- {result.OnlyInA.Count} mục chỉ có trong Nhóm A");
            Console.WriteLine($"- {result.OnlyInB.Count} mục chỉ có trong Nhóm B");
            Console.WriteLine($"- {result.Mismatches.Count} mục có mã giống nhau nhưng số tiền khác nhau");

            if (result.Matches.Any())
            {
                Console.WriteLine("\n--- MỤC TRÙNG KHỚP HOÀN TOÀN ---");
                foreach (var match in result.Matches)
                {
                    Console.WriteLine($"ID: {match.Id}, Tiền: {match.Amount:N0}");
                }
            }

            if (result.OnlyInA.Any())
            {
                Console.WriteLine("\n--- CHỈ CÓ TRONG NHÓM A ---");
                foreach (var item in result.OnlyInA)
                {
                    Console.WriteLine($"ID: {item.Id}, Tiền: {item.Amount:N0}");
                }
            }

            if (result.OnlyInB.Any())
            {
                Console.WriteLine("\n--- CHỈ CÓ TRONG NHÓM B ---");
                foreach (var item in result.OnlyInB)
                {
                    Console.WriteLine($"ID: {item.Id}, Tiền: {item.Amount:N0}");
                }
            }

            if (result.Mismatches.Any())
            {
                Console.WriteLine("\n--- MÃ GIỐNG NHAU, SỐ TIỀN KHÁC NHAU ---");
                foreach (var mismatch in result.Mismatches)
                {
                    Console.WriteLine($"ID: {mismatch.Id}, Tiền Nhóm A: {mismatch.AmountA:N0}, Tiền Nhóm B: {mismatch.AmountB:N0}");
                }
            }
        }

        // Helper method to find the index of a column in keys collection
        private int FindColumnIndex(IEnumerable<string> keys, string columnName)
        {
            // First try to find exact match
            int index = 1;
            foreach (var key in keys)
            {
                if (key == columnName)
                    return index;
                    
                if (key.StartsWith($"{columnName}#"))
                    return int.Parse(key.Substring(columnName.Length + 1));
                    
                index++;
            }
            
            // Then look for keys ending with the column index
            var matchingKey = keys.FirstOrDefault(k => k.EndsWith($"#{columnName}") || k.Contains($"#{columnName}#"));
            if (matchingKey != null)
            {
                var parts = matchingKey.Split('#');
                if (parts.Length > 1 && int.TryParse(parts[1], out int colIndex))
                {
                    return colIndex;
                }
            }
            
            return 1; // Default to first column if not found
        }
        
        // Helper method to get column value by occurrence index
        private string GetColumnValueByIndex(Dictionary<string, object> record, string columnName, int occurrenceIndex)
        {
            // Since we have duplicate column names, we need to find the nth occurrence
            // For now, since the CSV reader only stores the last value for duplicate keys,
            // we'll need to work with what we have
            
            // If the record has the column name as key, return its value
            if (record.ContainsKey(columnName))
            {
                return record[columnName]?.ToString() ?? "";
            }
            
            // Try to find unique keys
            var uniqueKeys = record.Keys.Where(k => k.StartsWith($"{columnName}#")).ToList();
            if (uniqueKeys.Count >= occurrenceIndex)
            {
                var key = uniqueKeys[occurrenceIndex - 1];
                return record[key]?.ToString() ?? "";
            }
            
            return "";
        }
    }
}