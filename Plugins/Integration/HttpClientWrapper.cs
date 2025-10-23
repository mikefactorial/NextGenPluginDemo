using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins.Integration
{
    /// <summary>
    /// A wrapper class for HttpClient to send HTTP requests and receive responses.
    /// </summary>
    public class HttpClientWrapper
    {
        public HttpClientWrapper()
        {
        }

        /// <summary>
        /// Sends an HTTP request to the specified URI and returns the response as a string.
        /// </summary>
        /// <param name="uri">The URI to which the request is sent.</param>
        /// <param name="credentialType">The type of credentials used for authorization (e.g., "Bearer").</param>
        /// <param name="authCredentials">The authorization credentials.</param>
        /// <param name="requestBody">The content of the request body. If null, a GET request is made; otherwise, a POST request is made.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="Exception">Thrown when the HTTP response indicates a failure.</exception>
        public virtual string GetResponse(string uri, string credentialType, string authCredentials, HttpContent requestBody)
        {
            HttpMessageHandler handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler)
            {
                //2 minute timeout
                BaseAddress = new Uri(uri),
                Timeout = new TimeSpan(0, 2, 0)
            };

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            if (!string.IsNullOrEmpty(credentialType) && !string.IsNullOrEmpty(authCredentials))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"{credentialType} {authCredentials}");
            }

            HttpResponseMessage response = null;
            //make the call
            if (requestBody != null)
            {
                response = httpClient.PostAsync(uri, requestBody).GetAwaiter().GetResult();
            }
            else
            {
                response = httpClient.GetAsync(uri).GetAwaiter().GetResult();
            }
            if (!response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase} - {content}");
            }
            //return the response
            if (response != null)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            return string.Empty;
        }
    }
}
