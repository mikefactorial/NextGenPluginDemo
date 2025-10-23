using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins.Integration
{
    /// <summary>
    /// Represents the Azure configuration variables required for integration.
    /// </summary>
    public class AzureVariables
    {
        /// <summary>
        /// Gets or sets the Blob endpoint URL.
        /// </summary>
        public string BlobEndpoint = "";

        /// <summary>
        /// Gets or sets the name of the Blob container.
        /// </summary>
        public string BlobContainerName = "";

        /// <summary>
        /// Gets or sets the connection string for the Blob storage.
        /// </summary>
        public string BlobConnectionString = "";
    }
}
