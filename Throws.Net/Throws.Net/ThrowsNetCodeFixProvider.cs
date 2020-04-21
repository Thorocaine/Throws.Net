using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Throws.Net
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThrowsNetCodeFixProvider))]
    [Shared]
    public class ThrowsNetCodeFixProvider : CodeFixProvider
    {
        // const string title = "Add Thros";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ThrowsNetAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
            =>
                // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
                WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start)
                .Parent.AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .First();

            // Register a code action that will invoke the fix.
            //context.RegisterCodeFix(
            //    CodeAction.Create(
            //        title: title,
            //        createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
            //        equivalenceKey: title),
            //    diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add Throws",
                    c => AddThrows(context.Document, diagnostic, root),
                    "Add Throws"),
                diagnostic);
        }

        static Task<Document> AddThrows(Document document, Diagnostic diagnostic, SyntaxNode root)
        {
            var method = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
            
            var name = SyntaxFactory.ParseName("Throws");
            var arguments = SyntaxFactory.ParseAttributeArgumentList("(typeof(Exception))");
            var attribute = SyntaxFactory.Attribute(name, arguments);
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>().Add(attribute);
            var list = SyntaxFactory.AttributeList(attributeList);

            var newMethod = method.AddAttributeLists(list);
            var newRoot = root.ReplaceNode(method, newMethod);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        Task<Document> CatchException(Document document, Diagnostic diagnostic, SyntaxNode root)
        {
            var statement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<StatementSyntax>();
            var method = GetMethod(statement);

            var newRoot = root.ReplaceNode(statement, SyntaxFactory.Block(statement));
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        static SyntaxNode GetMethod(SyntaxNode statement)
            => statement.Parent switch
            {
                MethodDeclarationSyntax methodDeclaration => methodDeclaration,
                _ => GetMethod(statement.Parent)
            };
    }
}