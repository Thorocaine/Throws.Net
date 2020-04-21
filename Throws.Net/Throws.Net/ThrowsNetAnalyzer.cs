using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Throws.Net.Helpers;

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
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeThrow, SyntaxKind.ThrowStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var helper = new TypeHelpers(context);
            var invocation = (InvocationExpressionSyntax)context.Node;

            
            var symbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
            var methodSymbol = symbol as IMethodSymbol;
            var syntaxReference = methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault();
            
            var syntax = syntaxReference?.GetSyntax(context.CancellationToken);

            if (!(syntax is MethodDeclarationSyntax methodDeclaration)) return;
            var container = helper.GetRelevantContainer(invocation);
            if (container is null) return;
            if(helper.GetThrowsTypes(methodDeclaration).Any(IsCaughtBy(helper, container))) return;
                  
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        static Func<ITypeSymbol?, bool> IsCaughtBy(TypeHelpers helper, SyntaxNode container)
            => (exceptionType) => container switch
            {
                TryStatementSyntax ts when helper.DoesCatch(ts, exceptionType) => true,
                MethodDeclarationSyntax md when helper.HasThrowsAttribute(md, exceptionType) => true,
                _ => false
            };

        static void AnalyzeThrow(SyntaxNodeAnalysisContext context)
        {
            var helper = new TypeHelpers(context);
            var throwStatement = (ThrowStatementSyntax) context.Node;

            var exceptionType = helper.GetType(throwStatement.ChildNodes().FirstOrDefault());
            var container = helper.GetRelevantContainer(throwStatement);
            switch (container)
            {
                case TryStatementSyntax ts when helper.DoesCatch(ts, exceptionType):
                case MethodDeclarationSyntax md when helper.HasThrowsAttribute(md, exceptionType):
                    return;
                default:
                {
                    var diagnostic = Diagnostic.Create(Rule, throwStatement.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    break;
                }
            }
        }
    }
}