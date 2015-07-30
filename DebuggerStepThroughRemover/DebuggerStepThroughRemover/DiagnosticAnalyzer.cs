using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebuggerStepThroughRemover
{
    // TODO: add ReSharper solution file with default setttings of resharper/C#
    // ReSharper disable InconsistentNaming

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DebuggerStepThroughRemoverAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DebuggerStepThroughRemover";

        private static readonly LocalizableString Title = "Type name contains lowercase letters";
        private static readonly LocalizableString MessageFormat = "Type name '{0}' contains lowercase letters";
        private static readonly LocalizableString Description = "";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}