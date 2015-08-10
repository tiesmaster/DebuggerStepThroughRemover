using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using TestHelper;
using Xunit;

namespace DebuggerStepThroughRemover.Test
{
    public class AnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Analyzer_WithTestData_ShouldReportAttribute(string test, int line, int column)
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
            VerifyCSharpDiagnostic(test, expected);
        }

        public static TheoryData<string, int, int> TestData
            = new TheoryData<string, int, int>
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
}", 7, 16}};

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DebuggerStepThroughRemoverAnalyzer();
        }
    }
}