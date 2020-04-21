using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Throws.Net.Helpers
{
    internal class TypeHelpers
    {
        readonly SyntaxNodeAnalysisContext _context;
        CancellationToken Token => _context.CancellationToken;

        public TypeHelpers(SyntaxNodeAnalysisContext context) => _context = context;

        public ISymbol ThrowsType
            => _context.Compilation.GetTypeByMetadataName("Throws.Net.ThrowsAttribute");

        public IEnumerable<ITypeSymbol?> GetThrowsTypes(MethodDeclarationSyntax methodDeclaration)
            => methodDeclaration.AttributeLists.SelectMany(x => x.Attributes)
                .Select(x => x.ChildNodes().ToArray())
                .Where(x => x.Length == 2)
                .Where(x => IsType(x[0], ThrowsType))
                .Select(x => x[1] as AttributeArgumentListSyntax)
                .Select(x => x?.Arguments.FirstOrDefault()?.Expression as TypeOfExpressionSyntax)
                .Select(x => x?.Type)
                .Where(x => x != null)
                .Select(GetType);

        public bool HasThrowsAttribute(
            MethodDeclarationSyntax methodDeclaration,
            ITypeSymbol? exceptionType)
            =>
                // methodDeclaration.AttributeLists
                // .SelectMany(x => x.Attributes)
                // .Select(x => x.ChildNodes().ToArray())
                // .Where(x => x.Length == 2)
                // .Where(x => IsType(x[0], ThrowsType))
                // .Select(x => x[1] as AttributeArgumentListSyntax)
                // .Select(x => x?.Arguments.FirstOrDefault()?.Expression as TypeOfExpressionSyntax)
                // .Select(x => x?.Type)
                // .Where(x => x != null)
                // .Select(GetType)
                GetThrowsTypes(methodDeclaration).Any(x => DoesInherit(exceptionType, x));

        public ITypeSymbol? GetType(SyntaxNode? node)
        {
            var info = _context.SemanticModel.GetTypeInfo(node, Token);
            return info.Type;
        }

        bool IsType(SyntaxNode node, ISymbol type) => GetType(node)?.Equals(type) ?? false;

        public SyntaxNode? GetRelevantContainer(SyntaxNode statement)
            => statement.Parent switch
            {
                null => null,
                TryStatementSyntax tryStatement => tryStatement,
                MethodDeclarationSyntax methodDeclaration => methodDeclaration,
                _ => GetRelevantContainer(statement.Parent)
            };

        public bool DoesCatch(TryStatementSyntax statement, ITypeSymbol? type)
            => GetCaughtTypes(statement).Any(x => DoesInherit(type, x));

        public bool DoesInherit(ITypeSymbol? type, ITypeSymbol? from)
        {
            if (type is null) return false;
            if (type.Equals(from)) return true;
            return type.BaseType != null && DoesInherit(type.BaseType, from);
        }

        IEnumerable<ITypeSymbol?> GetCaughtTypes(TryStatementSyntax statement)
            => statement.Catches.Select(x => x.Declaration.Type).Select(GetType);
    }
}