namespace CsvCompareApp.Models
{
    public enum ComparisonType
    {
        TwoColumns = 1,
        GroupColumns = 2
    }

    public class ComparisonResult
    {
        public List<string> Differences { get; set; } = new List<string>();
        public List<string> OnlyInFirst { get; set; } = new List<string>();
        public List<string> OnlyInSecond { get; set; } = new List<string>();
        public List<string> AmountMismatches { get; set; } = new List<string>();
        public int TotalFirstGroup { get; set; }
        public int TotalSecondGroup { get; set; }
        public int PerfectMatches { get; set; }
    }

    public class ColumnConfiguration
    {
        public List<string> ColumnNames { get; set; } = new List<string>();
        public bool HasHeaderRecord { get; set; } = true;
        public string FilePath { get; set; } = "";
    }

    public class GroupColumnConfiguration
    {
        public string IdColumn { get; set; } = "";
        public string AmountColumn { get; set; } = "";
        public string GroupName { get; set; } = "";
    }
}