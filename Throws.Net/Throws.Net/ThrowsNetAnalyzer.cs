using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Throws.Net.Helpers;

namespace Throws.Net
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowsNetAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ThrowsNet";
        
        static readonly DiagnosticDescriptor Rule = Rules.CreateRule();
        static readonly DiagnosticDescriptor OverriderRule = Rules.CreateOverrideRule();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule, OverriderRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeThrow, SyntaxKind.ThrowStatement);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeOverride, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFromInterface, SyntaxKind.MethodDeclaration);
        }

        static void AnalyzeFromInterface(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax method)) return;
            var symbol = context.SemanticModel.GetDeclaredSymbol(method, context.CancellationToken);


            var baseMembers = symbol.ContainingType.Interfaces
                .SelectMany(x => x.GetMembers(symbol.Name))
                .Where(x => symbol.ContainingType.FindImplementationForInterfaceMember(x).Equals(symbol))
                .ToArray()
                ;
            
            if (!baseMembers.Any()) return;
            
            AnalyzeBaseMembers(context, method, baseMembers);
           
               
            
        }


        static void AnalyzeBaseMembers(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method,  IEnumerable<ISymbol> baseMembers)
        {
            var helper = new TypeHelpers(context);
            var baseThrows = baseMembers
                .SelectMany(x => x.DeclaringSyntaxReferences)
                .Where(x => x != null)
                .Select(x => x.GetSyntax(context.CancellationToken))
                .Select(x => x)
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(x => helper.GetThrowsTypes(x))
                .Where(x => x != null)
                .Cast<ITypeSymbol>()
                .ToArray();


            var myThrows = helper.GetThrowsTypes(method).ToArray();

            var baseNotThrown = baseThrows.Where(x => !myThrows.Any(y => helper.DoesInherit(y, x)));
            var thrownNotBase = myThrows.Where(y => !baseThrows.Any(x => helper.DoesInherit(y, x)));


            foreach (var typeSymbol in thrownNotBase)
            {
                var diagnostic = Diagnostic.Create(OverriderRule, method.GetLocation());
                context.ReportDiagnostic(diagnostic); 
            }

            foreach (var typeSymbol in baseNotThrown)
            {
                var dic = ImmutableDictionary.Create<string, string>().Add("Exception", typeSymbol.Name);
                var diagnostic = Diagnostic.Create(Rule, method.GetLocation(), properties: dic);
                context.ReportDiagnostic(diagnostic);
            }
        }

        static void AnalyzeOverride(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax method)) return;
            if(method.Modifiers.All(x => x.Text != "override")) return;

            // var symbol = context.SemanticModel.GetDeclaredSymbol(method, context.CancellationToken);
            // if (!symbol.IsOverride) return;

            var location = method.GetLocation().SourceSpan.Start;
            var baseMembers = context.SemanticModel.LookupBaseMembers(location).ToArray();
            if (!baseMembers.Any()) return;
            AnalyzeBaseMembers(context, method, baseMembers);

            
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
                return container switch
                {
                    TryStatementSyntax ts when helper.DoesCatch(ts, exceptionType) => false,
                    MethodDeclarationSyntax md when helper.HasThrowsAttribute(md, exceptionType) =>
                    false,
                    _ => true
                };
                
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