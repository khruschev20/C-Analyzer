// AnalyzerController.cs
using Microsoft.AspNetCore.Mvc;
using CodeAnalyzerWeb.Models;
using CodeAnalyzerWeb.Services;

namespace CodeAnalyzerWeb.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly RoslynAnalyzer _analyzer;

        public AnalyzerController(RoslynAnalyzer analyzer)
        {
            _analyzer = analyzer;
        }

        public IActionResult Index()
        {
            return View(new CodeAnalysisRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(CodeAnalysisRequest request)
        {
            if (!ModelState.IsValid)
                return View("Index", request);
            
            var result = await _analyzer.AnalyzeCodeAsync(request.Code);
            return View("AnalysisResult", result);
        }
    }
}