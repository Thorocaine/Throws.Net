using System;
using Throws.Net;

namespace CSharp_Standard_Sample
{
    public class Class1
    {
        public void Sample_None(bool flag, int value)
        {
            throw new Exception("Test");
        }

        [Throws(typeof(Exception))]
        public void Sample_Throws(bool flag, int value)
        {
            throw new ArgumentException("Test");

        }

        public void Sample_Tries(bool flag, int value)
        {
            try
            {
                throw new Exception("Test");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
