using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using TestHelper;

using Xunit;

namespace DebuggerStepThroughRemover.Test
{
    public class AnalyzerAndCodeFixerTests : CodeFixVerifier
    {
        [Fact]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Analyzer_WithTestData_ShouldReportAttribute(TestData testData)
        {
            var expectedLocation = testData.ExpectedDiagnositicLocation;
            var expected = new DiagnosticResult
            {
                Id = "DebuggerStepThroughRemover",
                Message = $"Type 'TypeName' is decorated with DebuggerStepThrough attribute",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", expectedLocation.Line, expectedLocation.Character)
                        }
            };
            VerifyCSharpDiagnostic(testData.BrokenSource, expected);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void CodeFixer_WithTestData_ShouldFixSource(TestData testData)
        {
            // TODO: post SO question how to handle this
            VerifyCSharpFix(testData.BrokenSource, testData.ExpectedFixedSource, null, allowNewCompilerDiagnostics: true);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly TheoryData<TestData> TestData
            = new TheoryData<TestData>
            {
                new TestData {
                    Description = "attribute with imported namespace, should not remove namespace",
                    BrokenSource =  @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerStepThrough]
    class TypeName
    {   
    }
}",
                    ExpectedFixedSource = @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}", ExpectedDiagnositicLocation = new LinePosition(6, 5)},
                new TestData {
                    Description = "attribute without imported namespace, should remove full attribute",
                    BrokenSource = @"
namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerStepThrough]
    class TypeName
    {   
    }
}",
                    ExpectedFixedSource = @"
namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}", ExpectedDiagnositicLocation = new LinePosition(4, 5)},
                new TestData {
                    Description =  "class with two attributes, should remove correct attribute",
                    BrokenSource = @"
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
                    ExpectedFixedSource = @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete]
    class TypeName
    {
    }
}", ExpectedDiagnositicLocation = new LinePosition(8, 5)},
                new TestData {
                    Description =  "class with two attributes between brackets, should keep the other attribute and brackets",
                    BrokenSource = @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete, DebuggerStepThrough]
    class TypeName
    {
    }
}",
                    ExpectedFixedSource = @"
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [Obsolete]
    class TypeName
    {
    }
}", ExpectedDiagnositicLocation = new LinePosition(7, 16)}};

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