using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using TestHelper;
using Xunit;

namespace DebuggerStepThroughRemover.Test
{
    // TODO: rename accordingly
    public class AnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Analyzer_WithTestData_ShouldReportAttribute(string brokenSource, string fixedSource, int line, int column)
        {
            var expected = new DiagnosticResult
            {
                Id = "DebuggerStepThroughRemover",
                Message = $"Type 'TypeName' is decorated with DebuggerStepThrough attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };
            VerifyCSharpDiagnostic(brokenSource, expected);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void CodeFixer_WithTestData_ShouldFixSource(string brokenSource, string fixedSource, int line, int column)
        {
            // TODO: post SO question how to handle this
            VerifyCSharpFix(brokenSource, fixedSource, null, allowNewCompilerDiagnostics: true);
        }

        // TODO: encapsulate the test data, so we have normal tests in the test runner displays
        public static TheoryData<string, string, int, int> TestData
            = new TheoryData<string, string, int, int>
            {
                {
                    // attribute with imported namespace, should not remove namespace
                    @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerStepThrough]
    class TypeName
    {   
    }
}",
                    @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}", 6, 5},
                {
                    // attribute without imported namespace, should remove full attribute 
                    @"
namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerStepThrough]
    class TypeName
    {   
    }
}",
                    @"
namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}", 4, 5},
                {
                    // class with two attributes, should remove correct attribute
                    @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete]
    [DebuggerStepThrough]
    class TypeName
    {
    }
}",
                    @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete]
    class TypeName
    {
    }
}", 8, 5},
                {
                    // class with two attributes between brackets, should keep the other attribute and brackets
                    @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete, DebuggerStepThrough]
    class TypeName
    {
    }
}",
                    @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete]
    class TypeName
    {
    }
}", 7, 16}};

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DebuggerStepThroughRemoverAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DebuggerStepThroughRemoverCodeFixProvider();
        }
    }
}