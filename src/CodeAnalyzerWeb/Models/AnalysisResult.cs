using System.Collections.Generic;
using System.Linq;

namespace CodeAnalyzerWeb.Models
{
    public class AnalysisResult
    {
        public bool HasErrors { get; set; }
        public string? ErrorMessage { get; set; }
        public int LineCount { get; set; }
        public int ClassCount { get; set; }
        public int MethodCount { get; set; }
        public List<CodeIssue> PotentialBugs { get; set; } = new();
        public List<MethodMetric> Methods { get; set; } = new();
        
        public List<MethodMetric> TopComplexMethods => Methods
            .OrderByDescending(m => m.Complexity)
            .Take(5)
            .ToList();
    }

    public class CodeIssue
    {
        public int Line { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class MethodMetric
    {
        public string Name { get; set; } = string.Empty;
        public int Complexity { get; set; }
        public int LinesOfCode { get; set; }
    }
}