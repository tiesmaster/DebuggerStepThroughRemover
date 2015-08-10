using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DebuggerStepThroughRemover
{
    // ReSharper disable InconsistentNaming

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DebuggerStepThroughRemoverCodeFixProvider)), Shared]
    public class DebuggerStepThroughRemoverCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove DebuggerStepThrough attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DebuggerStepThroughRemoverAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RemoveDebuggerStepThroughAttributeAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RemoveDebuggerStepThroughAttributeAsync(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
        {
            var attributeSyntax =
                classDeclarationSyntax.DescendantNodes()
                                      .OfType<AttributeSyntax>()
                                      .First(a => a.GetText().ToString() == "DebuggerStepThrough");

            var attributeIndex = classDeclarationSyntax.AttributeLists.IndexOf((AttributeListSyntax)attributeSyntax.Parent);
            var newclassDeclarationSyntax = classDeclarationSyntax
                .WithAttributeLists(classDeclarationSyntax.AttributeLists.RemoveAt(attributeIndex));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(classDeclarationSyntax, newclassDeclarationSyntax);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}