using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DebuggerStepThroughRemover
{
    // TODO: add ReSharper solution file with default setttings of resharper/C#
    // ReSharper disable InconsistentNaming

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DebuggerStepThroughRemoverAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DebuggerStepThroughRemover";

        private static readonly LocalizableString Title = "Type is decorated with DebuggerStepThrough attribute";
        private static readonly LocalizableString MessageFormat = "Type '{0}' is decorated with DebuggerStepThrough attribute";
        private static readonly LocalizableString Description = "";
        private const string Category = "Debugging";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static readonly string _targetAttributeName = nameof(DebuggerStepThroughAttribute).Replace(nameof(Attribute), string.Empty);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclarationSyntax, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclarationSyntax(SyntaxNodeAnalysisContext context)
        {
            var classDeclarationNode = (ClassDeclarationSyntax)context.Node;
            foreach(var attributeListSyntax in classDeclarationNode.AttributeLists)
            {
                foreach(var attributeSyntax in attributeListSyntax.Attributes)
                {
                    var classContainsTargetAttribute = attributeSyntax.Name.GetText().ToString().EndsWith(_targetAttributeName);
                    if (classContainsTargetAttribute)
                    {
                        ReportDiagnostic(context, attributeSyntax);
                    }
                }
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule,
                    DetermineDiagnosticTarget(attributeSyntax).GetLocation(),
                    GetClassName(attributeSyntax)));
        }

        private static string GetClassName(AttributeSyntax attributeSyntax)
        {
            var classNode = attributeSyntax.Ancestors().OfType<ClassDeclarationSyntax>().Single();
            return classNode.Identifier.Text;
        }

        private static CSharpSyntaxNode DetermineDiagnosticTarget(AttributeSyntax attributeNode)
        {
            var attributesParentNode = (AttributeListSyntax)attributeNode.Parent;
            return attributesParentNode.Attributes.Count > 1
                ? (CSharpSyntaxNode) attributeNode
                : attributesParentNode;
        }
    }
}