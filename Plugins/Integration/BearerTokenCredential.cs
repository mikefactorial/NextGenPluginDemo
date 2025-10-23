using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins.Integration
{
    public class BearerTokenCredential : TokenCredential
    {
        private readonly string _token;
        public BearerTokenCredential(string token)
        {
            _token = token;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return new AccessToken(_token, DateTimeOffset.MaxValue);
        }

        public override async System.Threading.Tasks.ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return await System.Threading.Tasks.Task.FromResult(new AccessToken(_token, DateTimeOffset.MaxValue));
        }
    }
}
