using System;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynTestKit;
using Xunit;

namespace Throws.Net.Tests
{
    public abstract class ThrowsNetAnalyzerTests : AnalyzerTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override DiagnosticAnalyzer CreateAnalyzer() => new ThrowsNetAnalyzer();

        protected string Id => ThrowsNetAnalyzer.DiagnosticId;
    }

    public class ThrowExceptionTests : ThrowsNetAnalyzerTests
    {
        public static IEnumerable<object[]> GetCorrectlyCaught()
        {
            yield return new object[] {"Exception(\"Test\")", "Exception"};
            yield return new object[] {"ArgumentNullException(\"myField\")", "Exception"};
            yield return new object[] {"ArgumentNullException(\"myField\")", "ArgumentNullException"};
            yield return new object[] {"NullReferenceException(\"myField\")", "Exception"};
            yield return new object[] {"NullReferenceException(\"myField\")", "NullReferenceException"};
        }

        public static IEnumerable<object[]> GetWronglyCaught()
        {
            yield return new object[] {"ArgumentNullException(\"myField\")", "NullReferenceException"};
            yield return new object[] {"NullReferenceException(\"myField\")", "ArgumentNullException"};
        }

        protected override IReadOnlyCollection<MetadataReference> References => new[]
        {
            ReferenceSource.FromType<ThrowsNetAnalyzer>()
        };

        [Theory]
        [InlineData("Exception(\"Test\")")]
        [InlineData("ArgumentNullException(\"myField\")")]
        [InlineData("NullReferenceException(\"myField\")")]
        public void Diagnostic_when_throwing_any_exception(string exception)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        public void Sample_None(bool flag, int value)
        {{
            [|throw new {exception};|]
        }}
    }}
}}
";
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(GetCorrectlyCaught))]
        public void No_diagnostics_if_the_the_method_is_marked_with_throws(string exception, string throws)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        [Throws(typeof({throws}))]
        public void Sample_None(bool flag, int value)
        {{
            throw new {exception};
        }}
    }}
}}
";
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(GetCorrectlyCaught))]
        public void No_diagnostics_if_the_the_method_catches(string exception, string catches)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        public void Sample_Tries(bool flag, int value)
        {{
            try
            {{
                throw new {exception};
            }}
            catch ({catches} ex)
            {{
                Console.WriteLine(ex);
            }}
        }}
    }}
}}
";
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(GetWronglyCaught))]
        public void Diagnostics_if_the_the_method_is_marked_with_throws_wrong_type(string exception, string throws)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        [Throws(typeof({throws}))]
        public void Sample_None(bool flag, int value)
        {{
            [|throw new {exception}|];
        }}
    }}
}}
";
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(GetWronglyCaught))]
        public void Diagnostics_if_the_the_method_catches_wrong_type(string exception, string catches)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        public void Sample_Tries(bool flag, int value)
        {{
            try
            {{
                [|throw new {exception}|];
            }}
            catch ({catches} ex)
            {{
                Console.WriteLine(ex);
            }}
        }}
    }}
}}
";
            HasDiagnostic(code, Id);
        }
    }
}
