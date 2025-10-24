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
            //positive, negative, or  neutral
            if (response.Results["AnalyzedSentiment"].ToString().ToLower() == "positive" || answerText.ToLower().Contains("low code") || answerText.ToLower().Contains("lowcode"))
            {
                bulbControlRequest.Pattern = new PatternSettings
                {
                    Type = PatternType.Pulse,
                    TransitionDurationMs = 500,
                    RepeatCount = 5
                };
                bulbControlRequest.Colors = new List<ColorStep>
                {
                    new ColorStep
                    {
                        Hex = "#00FF00",
                        DurationMs = 1000
                    }
                };
            }
            else if (response.Results["AnalyzedSentiment"].ToString().ToLower() == "negative" || answerText.ToLower().Contains("tradition") || answerText.ToLower().Contains("traditional"))
            {
                bulbControlRequest.Pattern = new PatternSettings
                {
                    Type = PatternType.Pulse,
                    TransitionDurationMs = 200,
                    RepeatCount = 5
                };
                bulbControlRequest.Colors = new List<ColorStep>
                {
                    new ColorStep
                    {
                        Hex = "#FF0000",
                        DurationMs = 500
                    }
                };
            }
            else
            {
                bulbControlRequest.Pattern = new PatternSettings
                {
                    Type = PatternType.Sequential,
                    TransitionDurationMs = 500,
                    RepeatCount = 3
                };
                bulbControlRequest.Colors = new List<ColorStep>
                {
                    new ColorStep
                    {
                        Hex = "#FFFF00",
                        DurationMs = 1000
                    }
                };
            }
            var bulbPayload = Newtonsoft.Json.JsonConvert.SerializeObject(bulbControlRequest);
            var bulbRequest = new OrganizationRequest("mf_BulbApi");
            bulbRequest.Parameters.Add("BulbApiPayload", bulbPayload);
            localPluginContext.PluginUserService.Execute(bulbRequest);
            localPluginContext.Trace("AnswerPlugin Execution Completed");
        }
    }
}
