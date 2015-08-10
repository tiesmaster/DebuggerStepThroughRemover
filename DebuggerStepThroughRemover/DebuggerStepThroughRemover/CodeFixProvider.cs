using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DebuggerStepThroughRemover
{
    // ReSharper disable InconsistentNaming

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DebuggerStepThroughRemoverCodeFixProvider)), Shared]
    public class DebuggerStepThroughRemoverCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove DebuggerStepThrough attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DebuggerStepThroughRemoverAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveNode(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Document> RemoveNode(Document oldDocument, SyntaxNode nodeToRemove, CancellationToken cancellationToken)
        {
            var oldNode = nodeToRemove.Parent;
            var newNode = oldNode.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);

            var oldRoot = await oldDocument.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(oldNode, newNode);

            return oldDocument.WithSyntaxRoot(newRoot);
        }
    }
}