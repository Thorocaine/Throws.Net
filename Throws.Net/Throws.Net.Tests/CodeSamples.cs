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
   internal static class CodeSamples
   {
       public static string GetInvocationOfThrowsMethod(string exception, string? myThrows= null, string? myCatch = null)
        {
            var attributeCode = string.IsNullOrEmpty(myThrows) ? null : $"[Throws(typeof({myThrows}))]";
            var (tryCode, catchCode) = string.IsNullOrEmpty(myCatch)
                ? ("", "")
                : (
                    "try\r\n            {\r\n            ",
                    $"\r\n        }}\r\n            catch ({myCatch} ex)\r\n            {{\r\n            }}"
                    );
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
            {tryCode}[|DangerZone()|];{catchCode}
        }}

        [Throws(typeof({exception}))]        
        void DangerZone() {{}}
    }}
}}
";
            return code;
        }
        
       public static string CreateInterfaceMethod(string? baseThrows = null, string? myThrows = null)
       {
           var baseAttribute = string.IsNullOrEmpty(baseThrows) ? "" : $"[Throws(typeof({baseThrows}))]";
           var myAttribute = string.IsNullOrEmpty(myThrows) ? "" : $"[Throws(typeof({myThrows}))]";
        
           return @$"
using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{{
    public interface IClassA
    {{
        {baseAttribute}
        void Test();
    }}
    
    public class ClassA: IClassA
    {{
        [|{myAttribute}
        public void Test()
        {{
        }}|]
    }}
}}
        ";        
       }  

       public static string CreateOverrideMethod(string? baseThrows = null, string? myThrows = null)
       {
        var baseAttribute = string.IsNullOrEmpty(baseThrows) ? "" : $"[Throws(typeof({baseThrows}))]";
        var myAttribute = string.IsNullOrEmpty(myThrows) ? "" : $"[Throws(typeof({myThrows}))]";
        
        return @$"
        using System;
        using Throws.Net;
        
        namespace CSharp_Standard_Sample
        {{
            public abstract class ClassB
            {{
                {baseAttribute}
                public abstract void Test();
            }}
            
            public class Class1 : ClassB
            {{
                [|{myAttribute}
                public override void Test()
                {{
                }}|]
            }}
        }}
        ";        
       }  
    }
}