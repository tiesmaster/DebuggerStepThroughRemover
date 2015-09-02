using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebuggerStepThroughRemover
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DebuggerStepThroughRemoverAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DebuggerStepThroughRemover";

        private static readonly string Title = "Type is decorated with DebuggerStepThrough attribute";
        private static readonly string MessageFormat = "Type '{0}' is decorated with DebuggerStepThrough attribute";
        private static readonly string Description = "";
        private const string Category = "Debugging";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
            isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttributeNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttributeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeNode = (AttributeSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;
            if (symbol.ContainingType.MetadataName == typeof(DebuggerStepThroughAttribute).Name)
            {
                ReportDiagnostic(context, attributeNode);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, AttributeSyntax matchingNode)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule,
                    DetermineDiagnosticTarget(matchingNode).GetLocation(),
                    GetClassName(matchingNode)));
        }

        private static string GetClassName(AttributeSyntax attributeNode)
        {
            var classNode = attributeNode.Ancestors().OfType<ClassDeclarationSyntax>().Single();
            return classNode.Identifier.Text;
        }

        private static CSharpSyntaxNode DetermineDiagnosticTarget(AttributeSyntax attributeNode)
        {
            var attributesParentNode = (AttributeListSyntax)attributeNode.Parent;
            return ShouldKeepSquareBrackets(attributesParentNode)
                ? (CSharpSyntaxNode) attributeNode
                : attributesParentNode;
        }

        private static bool ShouldKeepSquareBrackets(AttributeListSyntax attributesParentNode)
        {
            return attributesParentNode.Attributes.Count > 1;
        }
    }
}