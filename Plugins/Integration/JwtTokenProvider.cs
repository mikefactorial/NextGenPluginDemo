using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins.Integration
{
    public static class JwtProvider
    {
        private static readonly Dictionary<string, string> _tokenCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, DateTime> _expiryCache = new Dictionary<string, DateTime>();
        private static readonly object _lock = new object();

        public static string GetToken(IEnvironmentVariableService environmentVariableService, string environmentVariableConfiguration)
        {
            string variableValue = environmentVariableService.RetrieveEnvironmentVariableValue(environmentVariableConfiguration);
            var tokenConfig = JsonConvert.DeserializeObject<JwtTokenConfiguration>(variableValue);
            return GetToken(tokenConfig.TenantId, tokenConfig.ClientId, tokenConfig.ClientSecret);
        }

        public static string GetToken(string tenantId, string clientId, string clientSecret)
        {
            lock (_lock)
            {
                if (!_tokenCache.ContainsKey(clientId) || DateTime.UtcNow >= _expiryCache[clientId])
                {
                    var token = GetNewToken(tenantId, clientId, clientSecret).Result;
                    _tokenCache[clientId] = token.Item1;
                    _expiryCache[clientId] = token.Item2;
                }

                return _tokenCache[clientId];
            }
        }

        private static async Task<Tuple<string, DateTime>> GetNewToken(string tenantId, string clientId, string clientSecret)
        {
            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                      {
                          { "grant_type", "client_credentials" },
                          { "client_id", clientId },
                          { "client_secret", clientSecret },
                          { "scope", $"api://{clientId}/.default" }
                      });

                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var doc = JObject.Parse(json);

                var token = doc["access_token"].ToString();
                var expiresIn = int.Parse(doc["expires_in"]?.ToString() ?? "0"); // seconds  

                return new Tuple<string, DateTime>(token, DateTime.UtcNow.AddSeconds(expiresIn - 60));
            }
        }

        public class JwtTokenConfiguration
        {
            /// <summary>
            /// Gets or sets the Azure Active Directory tenant ID.
            /// </summary>
            /// <remarks>
            /// The tenant ID is the directory ID of the Azure AD instance, typically a GUID.
            /// It identifies the specific Azure AD tenant where the client application is registered.
            /// </remarks>
            public string TenantId { get; set; }

            /// <summary>
            /// Gets or sets the client application ID.
            /// </summary>
            /// <remarks>
            /// The client ID is the unique application (client) ID assigned to the app registration in Azure AD.
            /// It identifies the specific application requesting the token.
            /// </remarks>
            public string ClientId { get; set; }

            /// <summary>
            /// Gets or sets the client secret for authentication.
            /// </summary>
            /// <remarks>
            /// The client secret is used to authenticate the application.
            /// This value should be kept secure, never exposed in client-side code, and ideally stored
            /// in a secure configuration store or environment variable.
            /// </remarks>
            public string ClientSecret { get; set; }
        }
    }
}
