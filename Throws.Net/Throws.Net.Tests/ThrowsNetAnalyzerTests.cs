using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynTestKit;

namespace Throws.Net.Tests
{
    public abstract class ThrowsNetAnalyzerTests : AnalyzerTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override DiagnosticAnalyzer CreateAnalyzer() => new ThrowsNetAnalyzer();

        protected static string Id => ThrowsNetAnalyzer.DiagnosticId;

        protected override IReadOnlyCollection<MetadataReference> References => new[]
        {
            ReferenceSource.FromType<ThrowsNetAnalyzer>()
        };
    }
}