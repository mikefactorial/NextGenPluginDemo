using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using NextGenDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins
{
    internal class AnswerSentimentPlugin : PluginBase
    {
        public AnswerSentimentPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(AnswerSentimentPlugin))
        {
        }
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }
            var context = localPluginContext.PluginExecutionContext;
            var answerText = localPluginContext.TargetEntity.GetAttributeValue<string>("mf_answertext");
            localPluginContext.Trace(localPluginContext.PluginExecutionContext.MessageName + " AnswerPlugin Execution Started");
            var bulbControlRequest = new BulbControlRequest();

            var aiSentimentRequest = new OrganizationRequest("AISentiment");
            aiSentimentRequest.Parameters.Add("Text", answerText);
            var response = localPluginContext.PluginUserService.Execute(aiSentimentRequest);
            if (response.Results["AnalyzedSentiment"].ToString().ToLower() == "positive")
            {
                bulbControlRequest.Action = "Green";
            }
            else if (response.Results["AnalyzedSentiment"].ToString().ToLower() == "negative")
            {
                bulbControlRequest.Action = "Red";
            }
            else
            {
                bulbControlRequest.Action = "Blue";
            }
            
            var bulbPayload = Newtonsoft.Json.JsonConvert.SerializeObject(bulbControlRequest);
            var bulbRequest = new OrganizationRequest("mf_BulbApi");
            bulbRequest.Parameters.Add("BulbApiPayload", bulbPayload);
            localPluginContext.PluginUserService.Execute(bulbRequest);
            localPluginContext.Trace("AnswerPlugin Execution Completed");
        }
    }
}
