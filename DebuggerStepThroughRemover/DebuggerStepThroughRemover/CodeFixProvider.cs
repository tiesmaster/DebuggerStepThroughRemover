using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DebuggerStepThroughRemover
{
    // ReSharper disable InconsistentNaming

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DebuggerStepThroughRemoverCodeFixProvider)), Shared]
    public class DebuggerStepThroughRemoverCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove DebuggerStepThrough attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DebuggerStepThroughRemoverAnalyzer.DiagnosticId);

        private static readonly string _debuggerStepThroughAttributeName =
            nameof(DebuggerStepThroughAttribute).Replace(nameof(Attribute), string.Empty);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var classDeclarationNode = GetClassDeclarationNode(root, diagnostic.Location.SourceSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveDebuggerStepThroughAttributeAsync(context.Document, classDeclarationNode, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RemoveDebuggerStepThroughAttributeAsync(Document originalDocument,
            ClassDeclarationSyntax classDeclarationNode, CancellationToken cancellationToken)
        {
            var debuggerStepThroughAttribute = classDeclarationNode
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .First(IsDebuggerStepThroughAttribute);

            var indexToRemove = classDeclarationNode.AttributeLists.IndexOf((AttributeListSyntax)debuggerStepThroughAttribute.Parent);
            var newClassDeclarationNode = classDeclarationNode
                .WithAttributeLists(classDeclarationNode.AttributeLists.RemoveAt(indexToRemove));

            var root = await originalDocument.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(classDeclarationNode, newClassDeclarationNode);

            return originalDocument.WithSyntaxRoot(newRoot);
        }

        private static ClassDeclarationSyntax GetClassDeclarationNode(SyntaxNode root, TextSpan diagnosticSpan)
        {
            var tokenPointedToByDiagnostic = root.FindToken(diagnosticSpan.Start);
            return tokenPointedToByDiagnostic.Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
        }

        private static bool IsDebuggerStepThroughAttribute(AttributeSyntax attributeNode) =>
            attributeNode.Name.GetText().ToString().EndsWith(_debuggerStepThroughAttributeName);

    }
}