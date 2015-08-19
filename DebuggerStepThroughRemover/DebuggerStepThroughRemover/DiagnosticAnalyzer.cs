using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

        private static readonly LocalizableString Title =
            "Type is decorated with DebuggerStepThrough attribute";
        private static readonly LocalizableString MessageFormat =
            "Type '{0}' is decorated with DebuggerStepThrough attribute";
        private static readonly LocalizableString Description = "";
        private const string Category = "Debugging";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
            isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var debuggerStepThroughAttributeSymbol = GetDebuggerStepThroughAttributeSymbol(model);
            var matchingAttributes = FindMatchingNodes(model, debuggerStepThroughAttributeSymbol);
            CreateDiagnosticsFromMatchingNodes(context, matchingAttributes);
        }

        private static IEnumerable<AttributeSyntax> FindMatchingNodes(SemanticModel model, INamedTypeSymbol debuggerStepThroughAttributeSymbol)
        {
            return
                from attributeNode in GetAllAttributeNodesInModel(model)
                let attributeClassSymbol = GetAttributeClassSymbolFromModel(model, attributeNode)
                where debuggerStepThroughAttributeSymbol.Equals(attributeClassSymbol)
                select attributeNode;
        }

        private static INamedTypeSymbol GetDebuggerStepThroughAttributeSymbol(SemanticModel model)
        {
            var debuggerStepThroughAttributeType = typeof (DebuggerStepThroughAttribute);
            var mscorlib = GetMscorlib(model, debuggerStepThroughAttributeType.GetTypeInfo().Assembly);
            var assemblySymbol = (IAssemblySymbol) model.Compilation.GetAssemblyOrModuleSymbol(mscorlib);
            return assemblySymbol.GetTypeByMetadataName(debuggerStepThroughAttributeType.FullName);
        }

        private static MetadataReference GetMscorlib(SemanticModel model, Assembly mscorlibAssembly)
        {
            var mscorlibAssemblyName = mscorlibAssembly.GetName().Name;
            return model.Compilation.References.Single(x => x.Display.Contains(mscorlibAssemblyName));
        }

        private static IEnumerable<AttributeSyntax> GetAllAttributeNodesInModel(SemanticModel model)
        {
            var syntaxRoot = model.SyntaxTree.GetRoot();
            return syntaxRoot.DescendantNodes().OfType<AttributeSyntax>();
        }

        private static ITypeSymbol GetAttributeClassSymbolFromModel(SemanticModel model, AttributeSyntax attributeNode)
        {
            var attributeSymbolInfo = model.GetSymbolInfo(attributeNode);
            var constructorSymbol = (IMethodSymbol) attributeSymbolInfo.Symbol;
            return constructorSymbol.ReceiverType;
        }

        private static void CreateDiagnosticsFromMatchingNodes(SemanticModelAnalysisContext context, IEnumerable<AttributeSyntax> matchingNodes)
        {
            foreach (var matchingNode in matchingNodes)
            {
                ReportDiagnostic(context, matchingNode);
            }
        }

        private static void ReportDiagnostic(SemanticModelAnalysisContext context, AttributeSyntax matchingNode)
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