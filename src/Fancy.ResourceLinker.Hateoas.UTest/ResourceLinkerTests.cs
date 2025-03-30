using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fancy.ResourceLinker.Hateoas.UTest;

[TestClass]
public class ResourceLinkerTests
{
    class TestNestedResource : ResourceBase { };
    class TestResource : ResourceBase { public TestNestedResource? Nested { get; set; } };

    [TestMethod]
    public void AddLinks_WithValidResource_ShouldCallLinkStrategy()
    {
        // Arrange
        Mock<ILinkStrategy> linkStrategyMock = new Mock<ILinkStrategy>();
        linkStrategyMock.Setup(x => x.CanLinkType(typeof(TestResource))).Returns(true);
        ResourceLinker resourceLinker = new ResourceLinker([ linkStrategyMock.Object ]);
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();
        TestResource resource = new TestResource();

        // Act
        resourceLinker.AddLinks(resource, urlHelperMock.Object);

        // Assert
        linkStrategyMock.Verify(x => x.LinkResource(resource, urlHelperMock.Object), Times.Once);
    }

    [TestMethod]
    public void AddLinks_WithNestedResource_ShouldCallLinkStrategyForNestedResource()
    {
        // Arrange
        Mock<ILinkStrategy> linkStrategyMock = new Mock<ILinkStrategy>();
        linkStrategyMock.Setup(x => x.CanLinkType(It.IsAny<Type>())).Returns(true);
        ResourceLinker resourceLinker = new ResourceLinker([linkStrategyMock.Object]);
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();
        var nestedResource = new TestNestedResource();
        var resource = new TestResource { Nested = nestedResource };

        // Act
        resourceLinker.AddLinks(resource, urlHelperMock.Object);

        // Assert
        linkStrategyMock.Verify(x => x.LinkResource(resource, urlHelperMock.Object), Times.Once);
        linkStrategyMock.Verify(x => x.LinkResource(nestedResource, urlHelperMock.Object), Times.Once);
    }

    [TestMethod]
    public void AddLinks_WithCollection_ShouldCallLinkStrategyForEachResource()
    {
        // Arrange
        Mock<ILinkStrategy> linkStrategyMock = new Mock<ILinkStrategy>();
        linkStrategyMock.Setup(x => x.CanLinkType(typeof(TestResource))).Returns(true);
        ResourceLinker resourceLinker = new ResourceLinker([linkStrategyMock.Object]);
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();
        TestResource[] resources = [new TestResource(), new TestResource()];

        // Act
        resourceLinker.AddLinks(resources, urlHelperMock.Object);

        // Assert
        linkStrategyMock.Verify(x => x.LinkResource(It.IsAny<TestResource>(), urlHelperMock.Object), Times.Exactly(2));
    }
}