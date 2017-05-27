using Xunit;
using Xunit.Sdk;

namespace SkippableTest
{
    [XunitTestCaseDiscoverer("SkippableTest.XunitExtensions.SkippableTheoryDiscoverer",
        "DotNetEditor.Tests")]
    public class SkippableTheoryAttribute : TheoryAttribute { }
}
