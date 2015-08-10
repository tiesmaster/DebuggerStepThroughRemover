using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace DebuggerStepThroughRemover.Test
{
    [TestClass]
    public class AnalyzerTests : DiagnosticVerifier
    {
        [TestMethod]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyzer_WithImportedNameSpace_ShouldReportAttribute()
        {
            var test = @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerStepThrough]
    class TypeName
    {   
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "DebuggerStepThroughRemover",
                Message = $"Type 'TypeName' is decorated with DebuggerStepThrough attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Analyzer_WithoutImportedNameSpace_ShouldReportAttribute()
        {
            var test = @"
namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerStepThrough]
    class TypeName
    {   
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "DebuggerStepThroughRemover",
                Message = $"Type 'TypeName' is decorated with DebuggerStepThrough attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DebuggerStepThroughRemoverAnalyzer();
        }
    }
}