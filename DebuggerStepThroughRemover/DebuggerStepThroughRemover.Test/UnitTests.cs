﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace DebuggerStepThroughRemover.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void WithImportedNameSpace_ShouldReportAttribute()
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

            var expected = new DiagnosticResult
            {
                Id = "DebuggerStepThroughRemover",
                Message = $"Type 'TypeName' is decorated with DebuggerStepThrough attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        [Ignore]
        public void OldTest()
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
                Message = $"Type name '{"TypeName"}' contains lowercase letters",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";

            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DebuggerStepThroughRemoverCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DebuggerStepThroughRemoverAnalyzer();
        }
    }
}