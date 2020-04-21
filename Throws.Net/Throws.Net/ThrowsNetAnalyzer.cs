using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
            var types = helper.GetThrowsTypes(methodDeclaration).ToArray();
            
            var uncaught = types
                .Where(IsNotCaughtBy(helper, container))
                .FirstOrDefault();
            if (uncaught is null) return;

            var dic = ImmutableDictionary.Create<string, string>().Add("Exception", uncaught.Name);
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), properties: dic);
            context.ReportDiagnostic(diagnostic);
        }

        static Func<ITypeSymbol?, bool> IsNotCaughtBy(TypeHelpers helper, SyntaxNode container)
            => (exceptionType) =>
            {
                var ressy = container switch
                {
                    TryStatementSyntax ts when helper.DoesCatch(ts, exceptionType) => false,
                    MethodDeclarationSyntax md when helper.HasThrowsAttribute(md, exceptionType) =>
                    false,
                    _ => true
                };
                return ressy;
            };

        static void AnalyzeThrow(SyntaxNodeAnalysisContext context)
        {
            var helper = new TypeHelpers(context);
            var throwStatement = (ThrowStatementSyntax) context.Node;

            var exceptionType = helper.GetType(throwStatement.ChildNodes().FirstOrDefault());
            var container = helper.GetRelevantContainer(throwStatement);
            
            if (container == null || !IsNotCaughtBy(helper, container)(exceptionType)) return;
            
            var dic = new ReadOnlyDictionary<string,string> (new Dictionary<string, string>
            {
                ["Exception"] = exceptionType?.Name ?? "Exception"
            });            
            var diagnostic = Diagnostic.Create(Rule, throwStatement.GetLocation(), dic);
            context.ReportDiagnostic(diagnostic);
            
        }
    }
}