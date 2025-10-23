using Azure.Storage.Blobs;
using NextGenDemo.Plugins.Integration;
using NextGenDemo.Plugins.Types;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace NextGenDemo.Plugins
{
    public sealed class InputParameters
    {
        public const string Target = "Target";
        public const string ColumnSet = "ColumnSet";
    }
    public sealed class OutputParameters
    {
        public const string EntityOutputParameterKey = "BusinessEntity";
    }

    /// <summary>
    /// Base class for all plug-in classes.
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public struct PluginMessage
        {
            public static string Assign = "Assign";
            public static string Delete = "Delete";
            public static string Update = "Update";
            public static string Create = "Create";
            public static string Retrieve = "Retrieve";
            public static string RetrieveMultiple = "RetrieveMultiple";
            public static string SetState = "SetState";
        }

        protected string PluginClassName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        /// <param name="pluginClassName">The <see cref="Type"/> of the plugin class.</param>
        internal PluginBase(Type pluginClassName)
        {
            PluginClassName = pluginClassName.ToString();
        }

        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Execute")]
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException(nameof(serviceProvider));
            }

            // Construct the local plug-in context.
            var localPluginContext = new LocalPluginContext(serviceProvider);

            localPluginContext.Trace($"Entered {PluginClassName}.Execute() " +
                $"Correlation Id: {localPluginContext.PluginExecutionContext.CorrelationId}, " +
                $"Initiating User: {localPluginContext.PluginExecutionContext.InitiatingUserId}");

            try
            {
                // Invoke the custom implementation
                ExecuteDataversePlugin(localPluginContext);

                // Now exit - if the derived plugin has incorrectly registered overlapping event registrations, guard against multiple executions.
                return;
            }
            catch (FaultException<OrganizationServiceFault> orgServiceFault)
            {
                localPluginContext.Trace($"Exception: {orgServiceFault.ToString()}");

                throw new InvalidPluginExecutionException($"OrganizationServiceFault: {orgServiceFault.Message}", orgServiceFault);
            }
            finally
            {
                localPluginContext.Trace($"Exiting {PluginClassName}.Execute()");
            }
        }


        /// <summary>
        /// Placeholder for a custom plug-in implementation.
        /// </summary>
        /// <param name="localPluginContext">Context for the current plug-in.</param>
        protected virtual void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            // Do nothing.
        }
    }

    /// <summary>
    /// This interface provides an abstraction on top of IServiceProvider for commonly used PowerPlatform Dataverse Plugin development constructs
    /// </summary>
    public interface ILocalPluginContext
    {
        /// <summary>
        /// The PowerPlatform Dataverse organization service for the Current Executing user.
        /// </summary>
        IOrganizationService InitiatingUserService { get; }

        /// <summary>
        /// The PowerPlatform Dataverse organization service for the Account that was registered to run this plugin, This could be the same user as InitiatingUserService.
        /// </summary>
        IOrganizationService PluginUserService { get; }

        /// <summary>
        /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
        /// </summary>
        IPluginExecutionContext PluginExecutionContext { get; }

        /// <summary>
        /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/>
        /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
        /// </summary>
        IServiceEndpointNotificationService NotificationService { get; }

        /// <summary>
        /// Provides logging run-time trace information for plug-ins.
        /// </summary>
        ITracingService TracingService { get; }

        /// <summary>
        /// General Service Provide for things not accounted for in the base class.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// OrganizationService Factory for creating connection for other then current user and system.
        /// </summary>
        IOrganizationServiceFactory OrgSvcFactory { get; }

        /// <summary>
        /// Blob Container Client for getting and setting blobs.
        /// </summary>
        BlobContainerClient BlobContainer { get; }

        /// <summary>
        /// ILogger for this plugin.
        /// </summary>
        ILogger Logger { get;  }

        IEnvironmentVariableService EnvironmentVariableService { get; }
        /// <summary>
        /// The current entity is the representation of the entity as it exists at this point in time including updated attributes and existing attributes
        /// </summary>
        Entity CurrentEntity { get; }

        Entity TargetEntity { get; }

        /// <summary>
        /// Writes a trace message to the trace log.
        /// </summary>
        /// <param name="message">Message name to trace.</param>
        void Trace(string message, [CallerMemberName] string method = null);

        /// <summary>
        /// Managed Identity Service for the plugin
        /// </summary>
        IManagedIdentityService ManagedIdentityService { get; }

        /// <summary>
        /// HTTP Client to be used for HTTP calls
        /// </summary>
        HttpClientWrapper HttpClient { get; }
    }

    /// <summary>
    /// Plug-in context object.
    /// </summary>
    public class LocalPluginContext : ILocalPluginContext
    {
        /// <summary>
        /// The PowerPlatform Dataverse organization service for the Current Executing user.
        /// </summary>
        public IOrganizationService InitiatingUserService { get; }

        /// <summary>
        /// The PowerPlatform Dataverse organization service for the Account that was registered to run this plugin, This could be the same user as InitiatingUserService.
        /// </summary>
        public IOrganizationService PluginUserService { get; }

        /// <summary>
        /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
        /// </summary>
        public IPluginExecutionContext PluginExecutionContext { get; }

        /// <summary>
        /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/>
        /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
        /// </summary>
        public IServiceEndpointNotificationService NotificationService { get; }

        /// <summary>
        /// Provides logging run-time trace information for plug-ins.
        /// </summary>
        public ITracingService TracingService { get; }

        /// <summary>
        /// General Service Provider for things not accounted for in the base class.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// OrganizationService Factory for creating connection for other then current user and system.
        /// </summary>
        public IOrganizationServiceFactory OrgSvcFactory { get; }

        /// <summary>
        /// Environment Variable Service for getting environment variables.
        /// </summary>
        public IEnvironmentVariableService EnvironmentVariableService { get; }
        /// <summary>
        /// ILogger for this plugin.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Blob Container Client for getting and setting blobs.
        /// </summary>
        public BlobContainerClient BlobContainer { get;  }

        /// <summary>
        /// Configuration for connecting to azure services.
        /// </summary>
        public AzureVariables AzureConfiguration { get; }

        /// <summary>
        /// Http Client to be used for Web Calls
        /// </summary>
        public HttpClientWrapper HttpClient { get; }

        /// <summary>
        /// Managed Identity Service for things not accounted for in the base class.
        /// </summary>
        public IManagedIdentityService ManagedIdentityService { get; }

        /// <summary>
        /// Helper object that stores the services available in this plug-in.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException(nameof(serviceProvider));
            }

            ServiceProvider = serviceProvider;

            Logger = serviceProvider.Get<ILogger>();

            PluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();

            TracingService = new LocalTracingService(serviceProvider);

            NotificationService = serviceProvider.Get<IServiceEndpointNotificationService>();

            OrgSvcFactory = serviceProvider.Get<IOrganizationServiceFactory>();

            PluginUserService = serviceProvider.GetOrganizationService(PluginExecutionContext.UserId); // User that the plugin is registered to run as, Could be same as current user.

            InitiatingUserService = serviceProvider.GetOrganizationService(PluginExecutionContext.InitiatingUserId); //User who's action called the plugin.

            ManagedIdentityService = (IManagedIdentityService)serviceProvider.GetService(typeof(IManagedIdentityService));


            HttpClient = serviceProvider.Get<HttpClientWrapper>();
            if (HttpClient == null)
            {
                HttpClient = new HttpClientWrapper();
            }

            EnvironmentVariableService = serviceProvider.Get<IEnvironmentVariableService>();
            if (EnvironmentVariableService == null)
            {
                TracingService.Trace("EnvironmentVariableService is null");
                EnvironmentVariableService = new EnvironmentVariableService(HttpClient, ManagedIdentityService, PluginUserService, TracingService);
            }
            AzureConfiguration = serviceProvider.Get<AzureVariables>();
            if (AzureConfiguration == null)
            {
                TracingService.Trace("AzureConfiguration is null");
                AzureConfiguration = new AzureVariables();
                AzureConfiguration.BlobContainerName = EnvironmentVariableService.RetrieveEnvironmentVariableValue(NextGenDemo.Plugins.Types.EnvironmentVariableService.StorageContainerEnvironmentVariableName);
                AzureConfiguration.BlobEndpoint = EnvironmentVariableService.RetrieveEnvironmentVariableValue(NextGenDemo.Plugins.Types.EnvironmentVariableService.StorageEndpointVariableName);
            }

            BlobContainer = serviceProvider.Get<BlobContainerClient>();
            if (BlobContainer == null)
            {
                TracingService.Trace("BlobContainer is null");

                if(!string.IsNullOrEmpty(this.AzureConfiguration.BlobEndpoint))
                {
                    // Create a BlobServiceClient
                    var scopes = new List<string> { $"https://storage.azure.com/.default" };

                    var accessToken = ManagedIdentityService.AcquireToken(scopes);
                    var credential = new BearerTokenCredential(accessToken);
                    var uri = new Uri(this.AzureConfiguration.BlobEndpoint);
                    var blobServiceClient = new BlobServiceClient(uri, credential);
                    // Create a BlobContainerClient
                    BlobContainer = blobServiceClient.GetBlobContainerClient(this.AzureConfiguration.BlobContainerName);
                }
            }
        }

        /// <summary>
        /// Writes a trace message to the trace log.
        /// </summary>
        /// <param name="message">Message name to trace.</param>
        public void Trace(string message, [CallerMemberName] string method = null)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }

            if (method != null)
                TracingService.Trace($"[{method}] - {message}");
            else
                TracingService.Trace($"{message}");
        }


        public Entity TargetEntity
        {
            get
            {
                if (PluginExecutionContext.InputParameters.Contains(InputParameters.Target))
                {
                    return PluginExecutionContext.InputParameters[InputParameters.Target] as Entity;
                }
                return null;
            }
        }

        public Entity PreImageEntity
        {
            get
            {
                if (PluginExecutionContext.PreEntityImages.Count > 0)
                {
                    return PluginExecutionContext.PreEntityImages.First().Value;
                }
                return null;
            }
        }

        private Entity currentEntity = null;
        public Entity CurrentEntity
        {
            get
            {
                if (currentEntity == null && this.TargetEntity != null)
                {
                    currentEntity = this.TargetEntity;
                    if (this.PreImageEntity != null)
                    {
                        currentEntity = this.PreImageEntity;
                        foreach (var att in this.TargetEntity.Attributes)
                        {
                            currentEntity[att.Key] = att.Value;
                        }
                    }
                }
                return currentEntity;
            }
        }
    }

    /// <summary>
    /// Specialized ITracingService implementation that prefixes all traced messages with a time delta for Plugin performance diagnostics
    /// </summary>
    public class LocalTracingService : ITracingService
    {
        private readonly ITracingService _tracingService;

        private DateTime _previousTraceTime;

        public LocalTracingService(IServiceProvider serviceProvider)
        {
            DateTime utcNow = DateTime.UtcNow;

            var context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));

            DateTime initialTimestamp = context.OperationCreatedOn;

            if (initialTimestamp > utcNow)
            {
                initialTimestamp = utcNow;
            }

            _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            _previousTraceTime = initialTimestamp;
        }

        public void Trace(string message, params object[] args)
        {
            var utcNow = DateTime.UtcNow;

            // The duration since the last trace.
            var deltaMilliseconds = utcNow.Subtract(_previousTraceTime).TotalMilliseconds;

            try
            {

                if (args == null || args.Length == 0)
                    _tracingService.Trace($"[+{deltaMilliseconds:N0}ms] - {message}");
                else
                    _tracingService.Trace($"[+{deltaMilliseconds:N0}ms] - {string.Format(message, args)}");
            }
            catch (FormatException ex)
            {
                throw new InvalidPluginExecutionException($"Failed to write trace message due to error {ex.Message}", ex);
            }
            _previousTraceTime = utcNow;
        }
    }
}
