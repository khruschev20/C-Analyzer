// CodeAnalysisRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CodeAnalyzerWeb.Models
{
    public class CodeAnalysisRequest
    {
        [Required]
        [Display(Name = "C# Code")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Analysis Type")]
        public AnalysisType AnalysisType { get; set; } = AnalysisType.SyntaxCheck;
    }

    public enum AnalysisType
    {
        SyntaxCheck,
        CodeMetrics,
        PotentialBugs
    }
}