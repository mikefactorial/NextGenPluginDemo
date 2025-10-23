using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using NextGenDemo.Plugins.Integration;
using NextGenDemo.Plugins.Types;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System.IO.Compression;
using Xunit;

namespace NextGenDemo.Plugins.Tests
{
    public class PluginTestsBase
    {
        [Fact]
        public void TestLocalPluginContext()
        {
            var mocks = SetupMocks();
            var localPluginContext = new Mock<LocalPluginContext>(mocks.serviceProvider.Object);

            localPluginContext.Object.TracingService.Should().NotBeNull();
            localPluginContext.Object.PluginUserService.Should().NotBeNull();
            localPluginContext.Object.InitiatingUserService.Should().NotBeNull();
        }
        public virtual (Mock<IServiceProvider> serviceProvider, Mock<IOrganizationService> organizationService, Mock<BlobContainerClient> blobContainerClient, Mock<BlobClient> blobClient, RemoteExecutionContext pluginContext) SetupMocks()
        {
            var organizationService = new Mock<IOrganizationService>();
            var organizationServiceFactory = new Mock<IOrganizationServiceFactory>();
            organizationServiceFactory.Setup(x => x.CreateOrganizationService(It.IsAny<Guid>())).Returns(organizationService.Object);
            var serviceProvider = new Mock<IServiceProvider>();
            var pluginContext = new RemoteExecutionContext();
            var tracingService = new MockTracingService();
            var blobContainerClient = new Mock<BlobContainerClient>();
            var blobClient = new Mock<BlobClient>();
            var mockManagedIdentityService = new Mock<IManagedIdentityService>();

            var mockHttpClient = new Mock<HttpClientWrapper>();
            serviceProvider.Setup(x => x.GetService(typeof(HttpClientWrapper))).Returns(mockHttpClient.Object);

            var environmentVariableService = new Mock<EnvironmentVariableService>(mockHttpClient.Object, mockManagedIdentityService.Object, organizationService.Object, tracingService);
            environmentVariableService.Setup(x => x.RetrieveEnvironmentVariableValue(It.IsAny<string>())).Returns("Test");
            serviceProvider.Setup(x => x.GetService(typeof(IEnvironmentVariableService))).Returns(environmentVariableService.Object);

            mockManagedIdentityService.Setup(m => m.AcquireToken(It.IsAny<List<string>>())).Returns("123");
            blobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);
            serviceProvider.Setup(x => x.GetService(typeof(BlobContainerClient))).Returns(blobContainerClient.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IManagedIdentityService))).Returns(mockManagedIdentityService.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IPluginExecutionContext))).Returns(pluginContext);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationServiceFactory))).Returns(organizationServiceFactory.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IOrganizationService))).Returns(organizationService.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IExecutionContext))).Returns(pluginContext);
            serviceProvider.Setup(x => x.GetService(typeof(ITracingService))).Returns(tracingService);

            /*
            organizationService.Setup(x => x.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>())
            ).Returns((string entityName, Guid id, ColumnSet columnSet) =>
            {
                var entity = new Entity(entityName);

                if (entityName == annotation.EntityName)
                {
                    entity[annotation.PrimaryName] = "Test";
                }
                return entity;
            });
            */
            return (serviceProvider, organizationService, blobContainerClient, blobClient, pluginContext);
        }
    }
}