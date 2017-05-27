using Xunit;
using Xunit.Sdk;

namespace SkippableTest
{
    [XunitTestCaseDiscoverer("SkippableTest.XunitExtensions.SkippableFactDiscoverer",
        "DotNetEditor.Tests")]
    public class SkippableFactAttribute : FactAttribute { }
}
