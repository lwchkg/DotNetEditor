using System;

namespace SkippableTest
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason)
            : base(reason) { }
    }
}
