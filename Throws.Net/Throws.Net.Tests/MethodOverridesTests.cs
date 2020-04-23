using Xunit;

namespace Throws.Net.Tests
{
    public class MethodInterfaceTests : ThrowsNetAnalyzerTests
    {
        [Theory]
        [InlineData("", "Exception")]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostic_when_overriding_method(string baseThrows, string myThrows)
        {
            var code = CodeSamples.CreateInterfaceMethod(baseThrows, myThrows);
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnosis_if_override_Throws_derived_type(string myThrows, string baseThrows)
        {
            var code = CodeSamples.CreateInterfaceMethod(baseThrows, myThrows);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [InlineData("", "Exception")]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnosis_if_override_not_throws_derived_type(string myThrows, string baseThrows)
        {
            var code = CodeSamples.CreateInterfaceMethod(baseThrows, myThrows);
            HasDiagnostic(code, Id);
        }

        
    }



    public class MethodOverridesTests : ThrowsNetAnalyzerTests
    {
        [Theory]
        [InlineData("", "Exception")]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnostic_when_overriding_method(string baseThrows, string myThrows)
        {
            var code = CodeSamples.CreateOverrideMethod(baseThrows, myThrows);
            HasDiagnostic(code, Id);
        }

        [Theory]
        [MemberData(nameof(TestData.GetCorrectlyCaught), MemberType = typeof(TestData))]
        public void No_diagnosis_if_override_Throws_derived_type(string myThrows, string baseThrows)
        {
            var code = CodeSamples.CreateOverrideMethod(baseThrows, myThrows);
            NoDiagnostic(code, Id);
        }

        [Theory]
        [InlineData("", "Exception")]
        [MemberData(nameof(TestData.GetWronglyCaught), MemberType = typeof(TestData))]
        public void Diagnosis_if_override_not_throws_derived_type(string myThrows, string baseThrows)
        {
            var code = CodeSamples.CreateOverrideMethod(baseThrows, myThrows);
            HasDiagnostic(code, Id);
        }

        
    }
}