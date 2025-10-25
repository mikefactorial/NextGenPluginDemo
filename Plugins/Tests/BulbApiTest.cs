using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Moq;
using Newtonsoft.Json;
using NextGenDemo.Plugins.Integration;
using NextGenDemo.Plugins.Types;
using NextGenDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace NextGenDemo.Plugins.Tests
{
    public class BulbApiTest : PluginTestsBase
    {
        private class TestableBulbApi : BulbApi
        {
            public TestableBulbApi(string unsecureConfiguration, string secureConfiguration)
                : base(unsecureConfiguration, secureConfiguration)
            {
            }

            public new void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
            {
                base.ExecuteDataversePlugin(localPluginContext);
            }
        }

        [Fact]
        public void BulbApi_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            var plugin = new BulbApi("unsecureConfig", "secureConfig");

            // Assert
            plugin.Should().NotBeNull();
        }

        [Fact]
        public void ExecuteDataversePlugin_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Arrange
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => plugin.ExecuteDataversePlugin(null!));
        }

        [Fact]
        public void ExecuteDataversePlugin_WithValidPayload_ShouldExecuteSuccessfully()
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            // Setup the input parameters
            var bulbRequest = new BulbControlRequest
            {
                BulbIP = "192.168.1.100",
                Action = "on",
                Colors = new List<ColorStep>
                {
                    new ColorStep { Hex = "#FF0000", DurationMs = 1000 }
                }
            };
            
            var jsonPayload = JsonConvert.SerializeObject(bulbRequest);
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = jsonPayload;

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert
            mocks.pluginContext.OutputParameters.Should().ContainKey(BulbApi.ResultOutputKey);
        }


        [Fact]
        public void ExecuteDataversePlugin_WithQuickAction_ShouldUseQuickActionUrl()
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            var bulbRequest = new BulbControlRequest
            {
                Action = "on" // Quick action
            };
            
            var jsonPayload = JsonConvert.SerializeObject(bulbRequest);
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = jsonPayload;

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext, useQuickAction: true);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert - Check that the quick action URL was used
            var mockEnvService = Mock.Get(mockLocalPluginContext.Object.EnvironmentVariableService);
            mockEnvService.Verify(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbQuickActionUrlName), Times.AtLeastOnce);
        }

        [Fact]
        public void ExecuteDataversePlugin_WithComplexPattern_ShouldUseControlUrl()
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            var bulbRequest = new BulbControlRequest
            {
                Action = "", // No quick action, should use control URL
                Colors = new List<ColorStep>
                {
                    new ColorStep { Hex = "#FF0000", DurationMs = 1000 },
                    new ColorStep { Hex = "#00FF00", DurationMs = 1000 }
                },
                Pattern = new PatternSettings
                {
                    Type = PatternType.Sequential,
                    RepeatCount = 3
                }
            };
            
            var jsonPayload = JsonConvert.SerializeObject(bulbRequest);
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = jsonPayload;

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext, useQuickAction: false);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert - Check that the control URL was used
            var mockEnvService = Mock.Get(mockLocalPluginContext.Object.EnvironmentVariableService);
            mockEnvService.Verify(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbControlUrlName), Times.AtLeastOnce);
        }

        [Fact]
        public void ExecuteDataversePlugin_WithHttpClientException_ShouldThrowInvalidPluginExecutionException()
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            var bulbRequest = new BulbControlRequest { Action = "on" };
            var jsonPayload = JsonConvert.SerializeObject(bulbRequest);
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = jsonPayload;

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext, throwHttpException: true);

            // Act & Assert
            var exception = Assert.Throws<InvalidPluginExecutionException>(() => 
                plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object));
            
            exception.Message.Should().Contain("Network error");
        }
        [Fact]
        public void ExecuteDataversePlugin_WithInvalidJson_ShouldHandleGracefully()
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            // Invalid JSON payload
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = "invalid json {";

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext);

            // Act & Assert
            Assert.Throws<JsonReaderException>(() => 
                plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object));
        }

        [Theory]
        [InlineData("")]
        public void ExecuteDataversePlugin_WithEmptyOrNonQuickAction_ShouldUseControlUrl(string action)
        {
            // Arrange
            var mocks = SetupMocks();
            var plugin = new TestableBulbApi("unsecureConfig", "secureConfig");
            
            var bulbRequest = new BulbControlRequest { Action = action };
            var jsonPayload = JsonConvert.SerializeObject(bulbRequest);
            mocks.pluginContext.InputParameters[BulbApi.BulbApiPayloadKey] = jsonPayload;

            var mockLocalPluginContext = SetupMockLocalPluginContext(mocks.pluginContext, useQuickAction: false);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert
            var mockEnvService = Mock.Get(mockLocalPluginContext.Object.EnvironmentVariableService);
            mockEnvService.Verify(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbControlUrlName), Times.AtLeastOnce);
        }

        [Fact]
        public void BulbApiPayloadKey_ShouldHaveCorrectValue()
        {
            // Assert
            BulbApi.BulbApiPayloadKey.Should().Be("BulbApiPayload");
        }

        [Fact]
        public void ResultOutputKey_ShouldHaveCorrectValue()
        {
            // Assert
            BulbApi.ResultOutputKey.Should().Be("Result");
        }

        private Mock<ILocalPluginContext> SetupMockLocalPluginContext(
            RemoteExecutionContext pluginContext, 
            bool useQuickAction = false, 
            bool throwHttpException = false,
            bool throwTokenException = false)
        {
            // Setup environment variable service
            var mockEnvService = new Mock<IEnvironmentVariableService>();
            mockEnvService.Setup(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.AzureFunctionAuthScopeName))
                         .Returns("https://testscope/.default");
            mockEnvService.Setup(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbIpVariableName))
                         .Returns("192.168.1.100");
            mockEnvService.Setup(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbControlUrlName))
                         .Returns("https://testfunction.azurewebsites.net/api/bulbcontrol");
            mockEnvService.Setup(x => x.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbQuickActionUrlName))
                         .Returns("https://testfunction.azurewebsites.net/api/bulbquickaction");

            // Setup HttpClient mock
            var mockHttpClient = new Mock<HttpClientWrapper>();
            if (throwHttpException)
            {
                mockHttpClient.Setup(x => x.GetResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContent>()))
                             .Throws(new HttpRequestException("Network error"));
            }
            else
            {
                mockHttpClient.Setup(x => x.GetResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpContent>()))
                             .Returns("{\"status\":\"success\"}");
            }

            // Setup ManagedIdentity mock
            var mockManagedIdentity = new Mock<IManagedIdentityService>();
            if (throwTokenException)
            {
                var list = It.IsAny<List<string>>();
                mockManagedIdentity.Setup(x => x.AcquireToken(list))
                                  .Throws(new UnauthorizedAccessException("Token acquisition failed"));
            }
            else
            {
                var list = It.IsAny<List<string>>();
                mockManagedIdentity.Setup(x => x.AcquireToken(list))
                                  .Returns("test-token-123");
            }

            // Create LocalPluginContext mock
            var mockLocalPluginContext = new Mock<ILocalPluginContext>();
            mockLocalPluginContext.Setup(x => x.PluginExecutionContext).Returns(pluginContext);
            mockLocalPluginContext.Setup(x => x.EnvironmentVariableService).Returns(mockEnvService.Object);
            mockLocalPluginContext.Setup(x => x.HttpClient).Returns(mockHttpClient.Object);
            mockLocalPluginContext.Setup(x => x.ManagedIdentityService).Returns(mockManagedIdentity.Object);

            return mockLocalPluginContext;
        }
    }
}
