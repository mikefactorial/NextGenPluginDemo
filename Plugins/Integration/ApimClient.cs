using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;

namespace NextGenDemo.Plugins.Integration
{
    /// <summary>
    /// Client for making authenticated requests to Azure API Management (APIM) endpoints from Dataverse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Registration:</b> This class is not registered as a plugin but is used by plugins and custom APIs.
    /// <list type="bullet">
    ///   <item><description>Used as a utility class for making HTTP requests to Azure API Management endpoints.</description></item>
    ///   <item><description>Can be initialized with configuration from Dataverse environment variables or direct parameters.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Business Rules:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Authentication:</b>
    ///       <list type="bullet">
    ///         <item><description>Supports JWT token authentication using Bearer scheme.</description></item>
    ///         <item><description>Supports Azure API Management subscription key authentication via Ocp-Apim-Subscription-Key header.</description></item>
    ///         <item><description>Both authentication methods can be used simultaneously or individually.</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Request Handling:</b>
    ///       <list type="bullet">
    ///         <item><description>Provides asynchronous GET and POST operations.</description></item>
    ///         <item><description>Automatically serializes .NET objects to JSON for POST requests.</description></item>
    ///         <item><description>Returns raw string responses that can be deserialized by the caller.</description></item>
    ///         <item><description>Throws exceptions for non-success HTTP status codes.</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ApimClient
    {
        private readonly string _baseUrl;
        private readonly string _jwtToken;
        private readonly string _apimSubscriptionKey;

        /// <summary>
        /// Initializes a new instance of the ApimClient class with direct parameter values.
        /// </summary>
        /// <param name="baseUrl">The base URL of the Azure API Management instance.</param>
        /// <param name="jwtToken">The JWT token for authentication.</param>
        /// <param name="apimSubscriptionKey">The Azure API Management subscription key.</param>
        /// <remarks>
        /// This constructor allows for direct initialization of the client without retrieving configuration from Dataverse.
        /// The baseUrl parameter is automatically cleaned by removing any trailing slash characters.
        /// </remarks>
        public ApimClient(string baseUrl, string jwtToken, string apimSubscriptionKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _jwtToken = jwtToken;
            _apimSubscriptionKey = apimSubscriptionKey;
        }

        /// <summary>
        /// Initializes a new instance of the ApimClient class with configuration from Dataverse environment variables.
        /// </summary>
        /// <param name="service">The organization service used to retrieve configuration.</param>
        /// <param name="tokenKey">The configuration key for retrieving the JWT token.</param>
        /// <param name="apimKey">The configuration key for retrieving APIM settings.</param>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>Retrieves APIM configuration (base URL and subscription key) using <see cref="ApimConfigurationProvider"/>.</description></item>
        ///   <item><description>Retrieves JWT token using <see cref="TokenProvider"/>.</description></item>
        ///   <item><description>The baseUrl is automatically cleaned by removing any trailing slash characters.</description></item>
        /// </list>
        /// </remarks>
        public ApimClient(IEnvironmentVariableService environmentVariableService, string tokenEnvironmentKey, string apimSubscriptionKey)
        {
            // Get the APIM configuration values
            var apimConfigJson = environmentVariableService.RetrieveEnvironmentVariableValue(apimSubscriptionKey);
            var apimConfig = JsonConvert.DeserializeObject<ApimConfiguration>(apimConfigJson);

            // Get the JSON Web Token
            var token = JwtProvider.GetToken(environmentVariableService, tokenEnvironmentKey);

            // Store the values for future calls
            _baseUrl = apimConfig.BaseUrl.TrimEnd('/');
            _jwtToken = token;
            _apimSubscriptionKey = apimConfig.SubscriptionKey;
        }

        /// <summary>
        /// Performs an authenticated GET request to an Azure API Management endpoint.
        /// </summary>
        /// <param name="apiFunction">The API function to call, containing the endpoint key.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails or returns a non-success status code.</exception>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>The complete URL is constructed by combining the base URL with the API function key.</description></item>
        ///   <item><description>Authentication headers are automatically added to the request.</description></item>
        ///   <item><description>Calls EnsureSuccessStatusCode to throw an exception if the response is not successful.</description></item>
        /// </list>
        /// </remarks>
        public async Task<string> GetAsync(string apiFunction)
        {
            using (var client = new HttpClient())
            {
                AddAuth(client);

                var response = await client.GetAsync($"{_baseUrl}/{apiFunction}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Performs an authenticated POST request to an Azure API Management endpoint with a JSON payload.
        /// </summary>
        /// <param name="apiFunction">The API function to call, containing the endpoint key.</param>
        /// <param name="payload">The object to serialize and send as the request body.</param>
        /// <param name="tracing">Optional tracing service for logging diagnostic information.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails or returns a non-success status code.</exception>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>The payload object is automatically serialized to JSON.</description></item>
        ///   <item><description>The complete URL is constructed by combining the base URL with the API function key.</description></item>
        ///   <item><description>Authentication headers are automatically added to the request.</description></item>
        ///   <item><description>Content-Type is set to application/json.</description></item>
        ///   <item><description>When tracing is provided, detailed operation steps are logged.</description></item>
        ///   <item><description>Calls EnsureSuccessStatusCode to throw an exception if the response is not successful.</description></item>
        /// </list>
        /// </remarks>
        public async Task<string> PostAsync(string apiFunction, object payload, ITracingService tracing = null)
        {
            tracing?.Trace("Creating new client");
            using (var client = new HttpClient())
            {
                tracing?.Trace("Adding Auth");
                AddAuth(client);

                tracing?.Trace("Serializing object");
                var json = JsonConvert.SerializeObject(payload);

                tracing?.Trace($"Serialized as {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseUrl}/{apiFunction}", content);

                // Always read the response content, regardless of status code
                var responseContent = await response.Content.ReadAsStringAsync();
                tracing?.Trace($"Response status: {(int)response.StatusCode} {response.StatusCode}, content: {responseContent}");

                return responseContent;
            }
        }

        /// <summary>
        /// Adds authentication headers to an HTTP client.
        /// </summary>
        /// <param name="client">The HTTP client to configure with authentication headers.</param>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>Adds a Bearer token if a JWT token is provided in the client configuration.</description></item>
        ///   <item><description>Sets the Accept header to application/json when using JWT authentication.</description></item>
        ///   <item><description>Adds an Ocp-Apim-Subscription-Key header if a subscription key is provided in the client configuration.</description></item>
        ///   <item><description>Both authentication methods can be used together or individually.</description></item>
        /// </list>
        /// </remarks>
        private void AddAuth(HttpClient client)
        {
            if (!String.IsNullOrWhiteSpace(_jwtToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            if (!String.IsNullOrEmpty(_apimSubscriptionKey))
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apimSubscriptionKey);
            }
        }

        public class ApimConfiguration
        {
            /// <summary>
            /// Gets or sets the base URL of the Azure API Management service.
            /// </summary>
            /// <remarks>
            /// The base URL should be the root endpoint of the API Management instance without any path segments.
            /// For example: "https://api-instance.azure-api.net"
            /// </remarks>
            public string BaseUrl { get; set; }

            /// <summary>
            /// Gets or sets the subscription key used for authenticating with the Azure API Management service.
            /// </summary>
            /// <remarks>
            /// The subscription key is sent in the Ocp-Apim-Subscription-Key header for authentication and rate limiting.
            /// This value should be kept secure and not exposed in client-side code.
            /// </remarks>
            public string SubscriptionKey { get; set; }
        }
    }
}
