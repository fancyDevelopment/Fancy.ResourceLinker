namespace Fancy.ResourceLinker.Models.UTest;

[TestClass]
public class ResourceActionTests
{
    [TestMethod]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        var target = new ResourceAction("PUT", "http://foo.bar/baz");
        Assert.IsNotNull(target);
        target = new ResourceAction("POST", "http://foo.bar/baz");
        Assert.IsNotNull(target);
        target = new ResourceAction("DELETE", "http://foo.bar/baz");
        Assert.IsNotNull(target);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithGETVerb_ThrowsException()
    {
        var target = new ResourceAction("GET", "http://foo.bar/baz");
    }
}