using System;

namespace Throws.Net.Test
{
    internal class Test
    {
        public void Sample(bool flag, int value) { throw new Exception(); }
    }
}