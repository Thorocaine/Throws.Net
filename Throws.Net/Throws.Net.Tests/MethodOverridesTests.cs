using Xunit;

namespace Throws.Net.Tests
{
    public class MethodOverridesTests : ThrowsNetAnalyzerTests
    {
        [Fact]
        public void Diagnostic_when_overriding_method()
        {
            var code = @"
";
            HasDiagnostic(code, Id);
        }
    }
}