using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fancy.ResourceLinker.Hateoas.UTest;

[TestClass]
public class LinkStrategyBaseTests
{
    class TestResource : ResourceBase { };
    class InvalidResource : ResourceBase { };

    class TestLinkStrategy : LinkStrategyBase<TestResource>
    {
        public bool LinkResourceInternalCalled { get; private set; }

        protected override void LinkResourceInternal(TestResource resource, IUrlHelper urlHelper)
        {
            LinkResourceInternalCalled = true;
        }
    }

    [TestMethod]
    public void CanLinkType_WithMatchingType_ShouldReturnTrue()
    {
        // Arrange
        TestLinkStrategy linkStrategy = new TestLinkStrategy();

        // Act
        bool result = linkStrategy.CanLinkType(typeof(TestResource));

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanLinkType_WithNonMatchingType_ShouldReturnFalse()
    {
        // Arrange
        TestLinkStrategy linkStrategy = new TestLinkStrategy();

        // Act
        bool result = linkStrategy.CanLinkType(typeof(string));

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void LinkResource_WithValidResource_ShouldCallLinkResourceInternal()
    {
        // Arrange
        TestResource resource = new TestResource();
        TestLinkStrategy linkStrategy = new TestLinkStrategy();
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();

        // Act
        linkStrategy.LinkResource(resource, urlHelperMock.Object);

        // Assert
        Assert.IsTrue(linkStrategy.LinkResourceInternalCalled);
    }

    [TestMethod]
    public void LinkResource_WithInvalidResource_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var resource = new InvalidResource();
        TestLinkStrategy linkStrategy = new TestLinkStrategy();
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => 
            linkStrategy.LinkResource(resource, urlHelperMock.Object));
    }
}