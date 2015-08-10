using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace DebuggerStepThroughRemover.Test
{
    [TestClass]
    public class CodeFixerTests : CodeFixVerifier
    {
        // TODO: also remove the namespace, when it's not being used anymore

        [TestMethod]
        public void Fixer_WithImportedNameSpace_ShouldRemoveAttribute()
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

            var fixtest = @"
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TypeName
    {   
    }
}";

            // TODO: post SO question how to handle this
            VerifyCSharpFix(test, fixtest, null, allowNewCompilerDiagnostics: true);
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