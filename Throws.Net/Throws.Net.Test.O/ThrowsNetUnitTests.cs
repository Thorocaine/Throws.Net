using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Throws.Net.Test
{
    [TestClass]
    public class ThrowsNetAnalyzerTests 
    {
        [TestMethod]
        public void Method_Cannot_Just_Throw()
        {
            var test = @"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{
    public class Class1
    {
        public void Sample_None(bool flag, int value)
        {
            throw new Exception(""Test"");
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = "ThrowsNet",
                Message = "Throws or catch expected",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {new DiagnosticResultLocation("Test0.cs", 11, 13)}
            };
            // VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Method_Should_be_marked_as_Throws()
        {
            var test = @"
using System;
using Throws.Net;

namespace Samples
{
    public class Class1
    {
        [Throws(typeof(Exception))]
        public void Sample()
        {
            throw new Exception(""Test"");
        }
    }
}
";
            // VerifyCSharpDiagnostic(test);
        }
    }

    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "ThrowsNet",
                Message =
                    string.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {new DiagnosticResultLocation("Test0.cs", 11, 15)}
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
            => new ThrowsNetCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ThrowsNetAnalyzer();
    }
}