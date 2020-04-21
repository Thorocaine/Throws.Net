using System.Collections.Generic;
using Xunit;

namespace Throws.Net.Tests
{
    public class MethodThrowsTests : ThrowsNetAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(TestData.GetSampleExceptions), MemberType = typeof(TestData))]
        public void Diagnostic_when_calling_throws_method(string exception)
        {
            var code = @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        public void Test()
        {{
            [|DangerZone()|];
        }}

        [Throws(typeof(${exception}))]        
        void DangerZone() {{}}
    }}
}}
";
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostic_when_calling_throws_method_and_catch_wrong_type(
            string myCatches,
            string methodThrows)
        {
            var code = GetCaughtCode(myCatches, methodThrows);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostic_when_calling_throws_method_and_throws_wrong_type(
            string myThrows,
            string methodThrows)
        {
            var code = GetThrowsCode(myThrows, methodThrows);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnostic_when_calling_throws_method_and_catches(
            string myCatches,
            string methodThrows)
        {
            var code = GetCaughtCode(myCatches, methodThrows);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnostic_when_calling_throws_method_and_throws(
            string myThrows,
            string methodThrows)
        {
            var code = GetThrowsCode(myThrows, methodThrows);
            NoDiagnostic(code, Id);
        }

        static string GetCaughtCode(string myCatches, string methodThrows)
            => @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        public void Test()
        {{
            try {{
                [|DangerZone()|];
            }}
            catch (${myCatches} ex) {{
                Console.WriteLine(ex);
            }}
        }}

        [Throws(typeof(${methodThrows}))]        
        void DangerZone() {{}}
    }}
}}
";

        static string GetThrowsCode(string myThrows, string methodThrows)
            => @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public class Class1
    {{
        [Throws(typeof(${myThrows}))]
        public void Test()
        {{
            [|DangerZone()|];
        }}

        [Throws(typeof(${methodThrows}))]        
        void DangerZone() {{}}
    }}
}}
";
    }

    public static class TestData
    {
        public static IEnumerable<object[]> GetCorrectlyCaught()
        {
            yield return new object[] {"Exception", "Exception"};
            yield return new object[] {"ArgumentNullException", "Exception"};
            yield return new object[] {"ArgumentNullException", "ArgumentNullException"};
            yield return new object[] {"NullReferenceException", "Exception"};
            yield return new object[] {"NullReferenceException", "NullReferenceException"};
        }

        public static IEnumerable<object[]> GetSampleExceptions()
        {
            yield return new object[] {"Exception"};
            yield return new object[] {"ArgumentNullException"};
            yield return new object[] {"NullReferenceException"};
        }

        public static IEnumerable<object[]> GetWronglyCaught()
        {
            yield return new object[] {"ArgumentNullException", "NullReferenceException"};
            yield return new object[] {"NullReferenceException", "ArgumentNullException"};
        }
    }

    public class ThrowExceptionTests : ThrowsNetAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(TestData.GetSampleExceptions), MemberType = typeof(TestData))]
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
            [|throw new {exception}(""Message"");|]
        }}
    }}
}}
";
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostics_if_the_the_method_catches_wrong_type(
            string exception,
            string catches)
        {
            var code = GetCaughtCode(exception, catches);
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostics_if_the_the_method_is_marked_with_throws_wrong_type(
            string exception,
            string throws)
        {
            var code = GetThrowsCode(exception, throws);
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnostics_if_the_the_method_catches(string exception, string catches)
        {
            var code = GetCaughtCode(exception, catches);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnostics_if_the_the_method_is_marked_with_throws(
            string exception,
            string throws)
        {
            var code = GetThrowsCode(exception, throws);
            NoDiagnostic(code, Id);
        }

        static string GetCaughtCode(string exception, string catches)
            => @$"
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

        static string GetThrowsCode(string exception, string throws)
            => @$"
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
    }
}