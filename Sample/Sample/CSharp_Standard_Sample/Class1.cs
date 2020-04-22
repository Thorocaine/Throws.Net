using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{
    public abstract class ClassB {
        [Throws(typeof(Exception))]
        public abstract void MyTest();
    }

    public class Class1 : ClassB
    {
        
        public override void MyTest()
        {
            
        }

        public void InhertitedTest()
        {
            MyTest();
        }

        [Throws(typeof(ArgumentNullException))]
        public int Sample_None(bool flag, int value)
        {
            var bob = Sample_Throws(flag, value);
            return bob;
        }

        [Throws(typeof(ArgumentException))]
        public int Sample_Throws(bool flag, int value)
        {
            throw new ArgumentException("Test");

        }

        public void Sample_Tries(bool flag, int value)
        {
            try
            {
                throw new Exception("Test");
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
