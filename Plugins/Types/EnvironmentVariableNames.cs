using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins.Types
{
    public static class EnvironmentVariableNames
    {
        public const string TenantId = "hsl_TenantId";
        public const string ApprovalClientId = "hsl_ApprovalClientId";
        public const string ApprovalFlowUrl = "hsl_ApprovalFlowUrl";
        public const string UnsecureSecret = "hsl_ApprovalClientSecretUnsecure";
        public const string SecureSecret = "hsl_ApprovalClientSecretSecure";
    }
}
