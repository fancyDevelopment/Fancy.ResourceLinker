using Fancy.ResourceLinker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fancy.ResourceLinker.Hateoas.UTest;

[TestClass]
public class ControllerExtensionsTests
{
    class TestResource : ResourceBase { };

    class TestController: ControllerBase { };

    [TestMethod]
    public void LinkResource_WithValidResource_ShouldCallResourceLinker()
    {
        // Arrange
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();
        Mock<IUrlHelperFactory> urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        urlHelperFactoryMock.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        Mock<IResourceLinker> resourceLinkerMock = new Mock<IResourceLinker>();
        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IUrlHelperFactory))).Returns(urlHelperFactoryMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IResourceLinker))).Returns(resourceLinkerMock.Object);
        Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        TestController testController = new TestController();
        testController.ControllerContext = new ControllerContext();
        testController.ControllerContext.HttpContext = httpContextMock.Object;
        TestResource resource = new TestResource();

        // Act
        testController.LinkResource(resource);

        // Assert
        resourceLinkerMock.Verify(x => x.AddLinks(resource, urlHelperMock.Object), Times.Once);
    }

    [TestMethod]
    public void LinkResource_WithMissingResourceLinker_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IResourceLinker))).Returns(null);
        Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        var urlHelperMock = new Mock<IUrlHelper>();
        TestController testController = new TestController();
        testController.ControllerContext = new ControllerContext();
        testController.ControllerContext.HttpContext = httpContextMock.Object;
        TestResource resource = new TestResource();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => testController.LinkResource(resource));
    }

    [TestMethod]
    public void LinkResources_WithValidResources_ShouldCallResourceLinkerForEachResource()
    {
        // Arrange
        Mock<IUrlHelper> urlHelperMock = new Mock<IUrlHelper>();
        Mock<IUrlHelperFactory> urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        urlHelperFactoryMock.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        Mock<IResourceLinker> resourceLinkerMock = new Mock<IResourceLinker>();
        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IUrlHelperFactory))).Returns(urlHelperFactoryMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IResourceLinker))).Returns(resourceLinkerMock.Object);
        Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        TestController testController = new TestController();
        testController.ControllerContext = new ControllerContext();
        testController.ControllerContext.HttpContext = httpContextMock.Object;
        TestResource[] resources = [new TestResource(), new TestResource()];

        // Act
        testController.LinkResources(resources);

        // Assert
        resourceLinkerMock.Verify(x => x.AddLinks(resources, urlHelperMock.Object), Times.Once);
    }
}