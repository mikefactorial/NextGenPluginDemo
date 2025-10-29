using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using NextGenDemo.Plugins.Integration;
using NextGenDemo.Plugins.Types;
using NextGenDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace NextGenDemo.Plugins
{
    /// <summary>
    /// Represents the Attachment API for handling file operations related to shipping documents.
    /// </summary>
    public class BulbApi : PluginBase
    {
        public static string BulbApiPayloadKey = "BulbApiPayload";
        /// <summary>
        /// Key for the result output parameter.
        /// </summary>
        public static string ResultOutputKey = "Result";

        /// <summary>
        /// Initializes a new instance of the <see cref="BulbApi"/> class.
        /// </summary>
        /// <param name="unsecureConfiguration">The unsecure configuration.</param>
        /// <param name="secureConfiguration">The secure configuration.</param>
        public BulbApi(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(BulbApi))
        {
        }

        /// <summary>
        /// Executes the custom business logic for the Attachment API.
        /// </summary>
        /// <param name="localPluginContext">The local plugin context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localPluginContext"/> is null.</exception>
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }
            var context = localPluginContext.PluginExecutionContext;

            localPluginContext.Trace(localPluginContext.PluginExecutionContext.MessageName + " BulbApi Plugin Execution Started");

            // Define the auth scope for the Azure Function (e.g. clientid/.default)
            var azFunctionScope = localPluginContext.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.AzureFunctionAuthScopeName);
            var scopes = new List<string>
            {
                azFunctionScope
            };

            // Acquire token for the Azure Function
            localPluginContext.Trace($"Acquiring token for Azure Function: {azFunctionScope}");
            var token = localPluginContext.ManagedIdentityService.AcquireToken(scopes);

            // Retrieve the JSON payload from input parameters
            localPluginContext.Trace($"Token acquired. Preparing to call Bulb API: {token}");
            if (localPluginContext.PluginExecutionContext.InputParameters[BulbApiPayloadKey] != null)
            {
                localPluginContext.Trace($"Bulb API payload found. Preparing request...");
                var jsonPayloadString = localPluginContext.PluginExecutionContext.InputParameters[BulbApiPayloadKey].ToString();
                var enabledDevicesIp = RetrieveEnabledDevices(localPluginContext.PluginUserService);

                // Deserialize the JSON string to ensure it's properly formatted, then create HttpContent
                var payloadObject = JsonConvert.DeserializeObject<BulbControlRequest>(jsonPayloadString);

                var azFunctionUrl = localPluginContext.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbQuickActionUrlName);
                if (string.IsNullOrEmpty(payloadObject.Action))
                {
                    azFunctionUrl = localPluginContext.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbControlUrlName);
                }

                // Set the Bulb IP from environment variable

                /*
                var bulbIp = localPluginContext.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.BulbIpVariableName);
                */

                // Itterate over each device
                foreach (var enabledDeviceIp in enabledDevicesIp)
                {
                    payloadObject.BulbIP = enabledDeviceIp;
                    var httpContent = new StringContent(JsonConvert.SerializeObject(payloadObject), Encoding.UTF8, "application/json");

                    localPluginContext.Trace($"Calling Bulb API with payload {JsonConvert.SerializeObject(payloadObject)}...");
                    try
                    {
                        var response = localPluginContext.HttpClient.GetResponse(azFunctionUrl, "Bearer", token, httpContent);
                        localPluginContext.Trace("Bulb API call completed.");
                        localPluginContext.Trace($"Received response: {response}");
                        localPluginContext.PluginExecutionContext.OutputParameters[ResultOutputKey] = response;
                    }
                    catch (Exception ex)
                    {
                        localPluginContext.Trace($"Error calling Bulb API: {ex.ToString()}");
                        throw new InvalidPluginExecutionException(ex.ToString());
                    }
                }
            }
            else
            {
                localPluginContext.Trace($"No Bulb API payload found. Proceeding to call Device List API...");
                // No API run device list
                var azFunctionUrl = localPluginContext.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.DeviceListUrlName);
                localPluginContext.Trace($"Calling Device List API: {azFunctionUrl}");
                try
                {

                    var response = localPluginContext.HttpClient.GetResponse(azFunctionUrl, "Bearer", token, null);
                    localPluginContext.Trace("Device List call completed.");
                    localPluginContext.Trace($"Received response: {response}");
                    localPluginContext.PluginExecutionContext.OutputParameters[ResultOutputKey] = response;
                }
                catch (Exception ex)
                {
                    localPluginContext.Trace($"Error calling Device List API: {ex.ToString()}");
                    throw new InvalidPluginExecutionException(ex.ToString());
                }
            }

        }

        protected List<string> RetrieveEnabledDevices(IOrganizationService service)
        {
            var bulbIpAddressQuery = new QueryExpression("mf_smartplugdevice");
            bulbIpAddressQuery.ColumnSet = new ColumnSet("mf_ipaddress");
            bulbIpAddressQuery.Criteria.AddCondition("mf_ipaddress", ConditionOperator.NotNull);
            bulbIpAddressQuery.Criteria.AddCondition("mf_ison", ConditionOperator.Equal, true);
            var enabledDevices = service.RetrieveMultiple(bulbIpAddressQuery).Entities.ToList();

            return enabledDevices
                .Select(device => device.GetAttributeValue<string>("mf_ipaddress"))
                .ToList();
        }
    }
}
