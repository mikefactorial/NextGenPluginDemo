using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using NextGenDemo.Plugins.Types;
using NextGenDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextGenDemo.Plugins
{
    public class DeviceDataProvider : PluginBase
    {
        public DeviceDataProvider(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(DeviceDataProvider))
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;
            localPluginContext.Trace(localPluginContext.PluginExecutionContext.MessageName);
            switch (localPluginContext.PluginExecutionContext.MessageName)
            {
                case "Update":
                    ExecuteUpdate(localPluginContext);
                    break;
                case "Retrieve":
                    ExecuteRetrieve(localPluginContext);
                    break;
                case "RetrieveMultiple":
                    ExecuteRetrieveMultiple(localPluginContext);
                    break;
                default:
                    throw new NotImplementedException($"The message: {localPluginContext.PluginExecutionContext.MessageName} is not supported");
            }
        }

        protected virtual void ExecuteUpdate(ILocalPluginContext context)
        {
            Entity entity = context.TargetEntity;
        }
        protected virtual void ExecuteRetrieve(ILocalPluginContext context)
        {
            Entity entity = new Entity(context.PluginExecutionContext.PrimaryEntityName);
            var devices = GetDevices(context);
            //context.PluginExecutionContext.OutputParameters["BusinessEntity"] = devices.FirstOrDefault(;
        }

        protected virtual void ExecuteRetrieveMultiple(ILocalPluginContext context)
        {
            context.Trace("DeviceDataProvider RetrieveMultiple Execution Started");
            var query = context.PluginExecutionContext.InputParameters["Query"];
            if (query != null)
            {
                context.Trace("Processing RetrieveMultiple with Query");
                var devices = GetDevices(context);
                List<Entity> entities = new List<Entity>();
                foreach (var device in devices)
                {
                    Entity deviceEntity = new Entity(context.PluginExecutionContext.PrimaryEntityName);
                    deviceEntity.Id = Guid.NewGuid();
                    deviceEntity["mf_smartdeviceid"] = deviceEntity.Id;
                    deviceEntity["mf_name"] = device.Alias;
                    deviceEntity["mf_address"] = device.Ip;
                    deviceEntity["mf_softwareversion"] = device.SoftwareVersion;
                    deviceEntity["mf_model"] = device.Model;
                    deviceEntity["mf_isturnedon"] = device.On;

                    entities.Add(deviceEntity);
                }
                

                context.PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = new EntityCollection(entities);
            }
        }

        protected virtual List<DeviceInfo> GetDevices(ILocalPluginContext context)
        {
            // Define the auth scope for the Azure Function (e.g. clientid/.default)
            var azFunctionScope = context.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.AzureFunctionAuthScopeName);
            var scopes = new List<string>
                {
                    azFunctionScope
                };

            // Acquire token for the Azure Function
            context.Trace($"Acquiring token for Azure Function: {azFunctionScope}");
            var token = context.ManagedIdentityService.AcquireToken(scopes);

            // No API run device list
            var azFunctionUrl = context.EnvironmentVariableService.RetrieveEnvironmentVariableValue(EnvironmentVariableService.DeviceListUrlName);
            var response = context.HttpClient.GetResponse(azFunctionUrl, "Bearer", token, null);
            // Deserialize the request with enum string conversion
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            return JsonSerializer.Deserialize<List<DeviceInfo>>(response, options).OrderBy(d => d.Alias).ToList();
        }
    }
}
