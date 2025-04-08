using CodeAnalyzerWeb.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalyzerWeb.Services
{
    public class RoslynAnalyzer : IDisposable
    {
        public async Task<AnalysisResult> AnalyzeCodeAsync(string code)
        {
            var result = new AnalysisResult();
            
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = await syntaxTree.GetRootAsync();
                
                result.LineCount = code.Split('\n').Length;
                result.ClassCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
                result.MethodCount = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Count();
                
                AnalyzePotentialBugs(root, result);
                CalculateMethodMetrics(root, result);
            }
            catch (Exception ex)
            {
                result.HasErrors = true;
                result.ErrorMessage = $"Analysis Error: {ex.Message}";
            }
            
            return result;
        }

        private void AnalyzePotentialBugs(SyntaxNode root, AnalysisResult result)
        {
            var potentialBugs = new List<CodeIssue>();
            
            // Detect empty catch blocks
            var emptyCatches = root.DescendantNodes()
                .OfType<CatchClauseSyntax>()
                .Where(c => c.Block.Statements.Count == 0);
            
            foreach (var catchBlock in emptyCatches)
            {
                potentialBugs.Add(new CodeIssue {
                    Line = catchBlock.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Message = "Empty catch block - consider handling exceptions"
                });
            }
            
            // Detect possible null checks
            var nullChecks = root.DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Where(IsSuspiciousNullCheck);
            
            foreach (var check in nullChecks)
            {
                potentialBugs.Add(new CodeIssue {
                    Line = check.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Message = "Possible null reference check - consider using null-conditional operators"
                });
            }

            var emptyFinallyBlocks = root.DescendantNodes()
    .OfType<FinallyClauseSyntax>()
    .Where(f => f.Block?.Statements.Count == 0);

foreach (var finallyBlock in emptyFinallyBlocks)
{
    potentialBugs.Add(new CodeIssue {
        Line = finallyBlock.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
        Message = "Empty finally block - consider adding cleanup logic"
    });
}
            
            result.PotentialBugs = potentialBugs;
        }

        private bool IsSuspiciousNullCheck(BinaryExpressionSyntax binary)
        {
            return binary.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken) &&
                   binary.Right is LiteralExpressionSyntax literal &&
                   literal.IsKind(SyntaxKind.NullLiteralExpression);
        }

        private void CalculateMethodMetrics(SyntaxNode root, AnalysisResult result)
        {
            var methods = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(m => new MethodMetric {
                    Name = m.Identifier.Text,
                    Complexity = CalculateCyclomaticComplexity(m),
                    LinesOfCode = m.GetText().Lines.Count
                });
            
            result.Methods = methods.ToList();
        }

        private int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
        {
            var complexity = 1;
            var walker = new ComplexityWalker();
            walker.Visit(method);
            return complexity + walker.Complexity;
        }

        public void Dispose() {}
    }

    internal class ComplexityWalker : CSharpSyntaxWalker
    {
        public int Complexity { get; private set; }
        
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Complexity++;
            base.VisitIfStatement(node);
        }
        
        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            Complexity += node.Labels.Count;
            base.VisitSwitchSection(node);
        }
        
        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.Kind() == SyntaxKind.LogicalAndExpression || 
        node.Kind() == SyntaxKind.LogicalOrExpression)
        Complexity++;
    
    base.VisitBinaryExpression(node);
        }
    }
}