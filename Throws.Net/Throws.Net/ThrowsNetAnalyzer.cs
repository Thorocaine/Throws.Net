using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Throws.Net
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowsNetAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ThrowsNet";
        const string Category = "Usage";

        static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle),
            Resources.ResourceManager,
            typeof(Resources));

        static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.AnalyzerMessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

        static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.AnalyzerDescription),
            Resources.ResourceManager,
            typeof(Resources));

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeThrow, SyntaxKind.ThrowStatement);

            //context.RegisterSemanticModelAction(
            //    modelContext =>
            //    {


            //        var root = modelContext.SemanticModel.SyntaxTree.GetRoot(modelContext.CancellationToken);
            //        foreach (var statement in root.DescendantNodes().OfType<ThrowStatementSyntax>())
            //        {
            //            var si = modelContext.SemanticModel.GetSymbolInfo(statement);
            //            var i = si.Symbol;
            //            Console.WriteLine(i);
            //            //var mt = GetMethodOrTry(statement);
            //            //if (mt is TryStatementSyntax) return;
            //            //if (mt is MethodDeclarationSyntax mds && HasThrows(modelContext.SemanticModel, mds))
            //            //    return;

            //            //var diagnostic = Diagnostic.Create(
            //            //    Rule,
            //            //    statement.GetLocation());
            //            //modelContext.ReportDiagnostic(diagnostic);
            //        }
            //    });


            //context.RegisterSyntaxNodeAction(
            //    node =>
            //    {
            //        var mt = GetMethodOrTry(node.Node);
            //        if (mt is TryStatementSyntax) return;
            //        if (mt is MethodDeclarationSyntax mds && HasThrows(node.SemanticModel, mds))
            //            return;

            //        var diagnostic = Diagnostic.Create(
            //            Rule,
            //            node.Node.GetLocation());
            //        node.ReportDiagnostic(diagnostic);

            //        //// Iterate through all statements in the tree
            //        //var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);
            //        //foreach (var statement in root.DescendantNodes().OfType<ThrowStatementSyntax>())
            //        //{
            //        //    var mt = GetMethodOrTry(statement);
            //        //    if (mt is TryStatementSyntax) continue;
            //        //    if (mt is MethodDeclarationSyntax  mds && HasThrows(,  mds)) continue;

            //        //    var diagnostic = Diagnostic.Create(Rule, statement.GetFirstToken().GetLocation());
            //        //    syntaxTreeContext.ReportDiagnostic(diagnostic);
            //        //    //// Skip analyzing block statements 
            //        //    //if (statement is BlockSyntax)
            //        //    //{
            //        //    //    continue;
            //        //    //}

            //        //    //// Report issues for all statements that are nested within a statement
            //        //    //// but not a block statement
            //        //    //if (statement.Parent is StatementSyntax && !(statement.Parent is BlockSyntax))
            //        //    //{
            //        //    //    var diagnostic = Diagnostic.Create(Rule, statement.GetFirstToken().GetLocation());
            //        //    //    syntaxTreeContext.ReportDiagnostic(diagnostic);
            //        //    //}
            //    },
            //    SyntaxKind.ThrowStatement);
        }

        void AnalyzeThrow(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node; 
            //INamedTypeSymbol exceptionType = compilation.GetTypeByMetadataName("System.Exception");
            //Console.WriteLine(exceptionType);

            var exceptionType = context.SemanticModel.GetTypeInfo(throwStatement.ChildNodes().First()).ConvertedType;

            var container = GetMethodOrTry(throwStatement);
            if (container is TryStatementSyntax) return; // TODO: Check type of catch.
            if (container is MethodDeclarationSyntax methodDeclaration && HasThrows(context, methodDeclaration, exceptionType)) return;
            

            var diagnostic = Diagnostic.Create(Rule, throwStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);

            //var throwStatement = (ThrowStatementSyntax)context.Node;
            //var tt = InferTypeInThrowStatement(throwStatement);
            //Console.WriteLine(tt);
            //context.SemanticModel.GetSymbolInfo(throwStatement.)
            //var model = compilation.GetSemanticModel(tree);
        }

        bool HasThrows(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, ITypeSymbol exceptionType)
        {
            var throwsType = context.Compilation.GetTypeByMetadataName("Throws.Net.ThrowsAttribute");
            return methodDeclaration.AttributeLists.SelectMany(x => x.Attributes)
                .Select(x => x.ChildNodes().First())
                .Select(x => context.SemanticModel.GetTypeInfo(x).ConvertedType)
                .Any(x => throwsType.Equals(x));
            // ToDo: Check exception type
        }


        //private IEnumerable<ITypeSymbol> InferTypeInThrowStatement(ThrowStatementSyntax throwStatement, SyntaxToken? previousToken = null)
        //{
        //    // If we have a position, it has to be after the 'throw' keyword.
        //    if (previousToken.HasValue && previousToken.Value != throwStatement.ThrowKeyword)
        //    {
        //        return SpecializedCollections.EmptyEnumerable<ITypeSymbol>();
        //    }

        //    return SpecializedCollections.SingletonEnumerable(this.Compilation.ExceptionType());
        //}

        static void AnalyzeThrow(SymbolAnalysisContext context)
        {
            

            var namedTypeSymbol = (INamedTypeSymbol) context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(
                    Rule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol) context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(
                    Rule,
                    namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        static SyntaxNode? GetMethodOrTry(SyntaxNode statement)
            => statement.Parent switch
            {
                null => null,
                TryStatementSyntax tryStatement => tryStatement,
                MethodDeclarationSyntax methodDeclaration => methodDeclaration,
                _ => GetMethodOrTry(statement.Parent)
            };


    }
}