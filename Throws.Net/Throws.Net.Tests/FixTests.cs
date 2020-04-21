using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynTestKit;
using Xunit;

namespace Throws.Net.Tests
{
    public class FixTests : CodeFixTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;

        protected override CodeFixProvider CreateProvider() => new ThrowsNetCodeFixProvider();

        protected override IReadOnlyCollection<DiagnosticAnalyzer> CreateAdditionalAnalyzers()
        {
            var list = (base.CreateAdditionalAnalyzers() ?? Enumerable.Empty<DiagnosticAnalyzer>())
                .Concat(new[] {new ThrowsNetAnalyzer()})
                .ToList();
            return new ReadOnlyCollection<DiagnosticAnalyzer>(list);
        }

        protected override IReadOnlyCollection<MetadataReference> References
            => new[] {ReferenceSource.FromType<ThrowsNetAnalyzer>()};

        [Theory]
        [MemberData(nameof(TestData.GetSampleExceptions), MemberType = typeof(TestData))]
        public void Fix_adds_a_Throws_attribute(string exception)
        {
            var code = CodeSamples.GetInvocationOfThrowsMethod(exception);
            var fixedCode = CodeSamples.GetInvocationOfThrowsMethod(exception, myThrows: exception)
                .Replace("[|", "")
                .Replace("|]", "");
            this.TestCodeFix(code, fixedCode, ThrowsNetAnalyzer.DiagnosticId, 0);
        }




        
    }

   internal static class CodeSamples
   {
       
        public static string GetInvocationOfThrowsMethod(string exception, string? myThrows= null, string? myCatch = null)
        {
            var attributeCode = string.IsNullOrEmpty(myThrows) ? null : $"[Throws(typeof({myThrows}))]";
            var (tryCode, catchCode) = string.IsNullOrEmpty(myCatch)
                ? ((string?, string?))(null, null)
                : ("try {", $"catch({myCatch} ex) {{}}");
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        {attributeCode}
        public void Test()
        {{
            {tryCode}
            [|DangerZone()|];
            {catchCode}
        }}

        [Throws(typeof(${exception}))]        
        void DangerZone() {{}}
    }}
}}
";
            return code;
        }
    }
}