using NextGenDemo.Plugins.Types;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using NextGenDemo.Plugins.Integration;
using System;
using System.Collections.Generic;

namespace NextGenDemo.Plugins.Types
{
    public class EnvironmentVariableService : IEnvironmentVariableService
    {
        public static string StorageContainerEnvironmentVariableName = "mf_storagecontainername";
        public static string StorageEndpointVariableName = "mf_storageendpoint";
        public static string BulbIpVariableName = "mf_bulbip";
        public static string APIMAccessTokenName = "mf_apimaccesstoken";
        public static string DeviceListUrlName = "mf_devicelisturl";
        public static string BulbQuickActionUrlName = "mf_bulbquickactionurl";
        public static string BulbControlUrlName = "mf_bulbcontrolurl";
        public static string AzureFunctionAuthScopeName = "mf_azurefunctionauthscope";
        public EnvironmentVariableService(HttpClientWrapper httpClient, IManagedIdentityService managedIdentityService, IOrganizationService service, ITracingService tracingService)
        {
            this.HttpClient = httpClient;
            this.ManagedIdentityService = managedIdentityService;
            this.PluginUserService = service;
            this.TracingService = tracingService;
        }

        public IManagedIdentityService ManagedIdentityService { get; private set; }
        public HttpClientWrapper HttpClient { get; private set; }
        public IOrganizationService PluginUserService { get; private set; }
        public ITracingService TracingService { get; private set; }

        public virtual string RetrieveEnvironmentVariableValue(string schemaName)
        {
            this.TracingService.Trace($"Entered GetEnvironmentVariables");
            // Singleton pattern to load environment variables less
            var query = new QueryExpression(environmentvariabledefinition.EntityName)
            {
                ColumnSet = new ColumnSet(environmentvariabledefinition.statecode, environmentvariabledefinition.defaultvalue, environmentvariabledefinition.valueschema,
                    environmentvariabledefinition.PrimaryName, environmentvariabledefinition.PrimaryKey, environmentvariabledefinition.type),
                LinkEntities =
            {
                new LinkEntity
                {
                    JoinOperator = JoinOperator.LeftOuter,
                    LinkFromEntityName = environmentvariabledefinition.EntityName,
                    LinkFromAttributeName = environmentvariabledefinition.PrimaryKey,
                    LinkToEntityName = environmentvariablevalue.EntityName,
                    LinkToAttributeName = environmentvariabledefinition.PrimaryKey,
                    Columns = new ColumnSet(environmentvariablevalue.statecode, environmentvariablevalue.value, environmentvariablevalue.PrimaryKey),
                    EntityAlias = "v"
                }
            }
            };
            query.Criteria.AddCondition(environmentvariabledefinition.PrimaryName, ConditionOperator.Equal, schemaName);
            var results = this.PluginUserService.RetrieveMultiple(query);
            if (results?.Entities.Count > 0)
            {
                if (results.Entities[0].Contains(environmentvariabledefinition.type))
                {
                    var type = results.Entities[0].GetAttributeValue<OptionSetValue>(environmentvariabledefinition.type);
                    if (type != null && type.Value == (int)environmentvariabledefinition.type_OptionSet.Secret)
                    {
                        return GetKeyVaultSecret(results.Entities[0].GetAttributeValue<AliasedValue>($"v.{environmentvariablevalue.value}")?.Value?.ToString());
                    }
                }
                return results.Entities[0].GetAttributeValue<AliasedValue>($"v.{environmentvariablevalue.value}")?.Value?.ToString();
            }
            throw new ArgumentException($"The environment variable with schema name {schemaName} does not exist in the system.");
        }
        /// <summary>
        /// Retrieves a secret from Key Vault based on the configuration from an environment variable.
        /// </summary>
        /// <param name="secretEnvironmentVariableValue">The value of the environment variable which contains the path to the secret in Azure</param>
        /// <returns></returns>
        public string GetKeyVaultSecret(string secretEnvironmentVariableValue)
        {
            if (!string.IsNullOrEmpty(secretEnvironmentVariableValue))
            {
                var split = secretEnvironmentVariableValue.Split('/');
                var keyVaultPath = $"https://{split[split.Length - 3]}.vault.azure.net/secrets/{split[split.Length - 1]}?api-version=7.4";

                var scopes = new List<string> { $"https://vault.azure.net/.default" };
                var token = this.ManagedIdentityService.AcquireToken(scopes);
                var response = this.HttpClient.GetResponse(keyVaultPath, "Bearer", token, null);
                var mySecret = JsonConvert.DeserializeObject<KeyVaultSecret>(response);
                if (mySecret != null && mySecret.value.Trim().ToUpper() != "NA")
                {
                    return mySecret.value;
                }
            }
            return string.Empty;
        }

    }
}
