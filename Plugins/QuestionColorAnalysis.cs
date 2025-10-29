using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NextGenDemo.Plugins.Integration;
using NextGenDemo.Plugins.Types;
using NextGenDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NextGenDemo.Plugins
{
    public class QuestionColorAnalysis : PluginBase
    {
        public QuestionColorAnalysis(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(QuestionColorAnalysis))
        {
        }
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;
            tracingService.Trace("Entered ExecuteDataversePlugin for QuestionColorAnalysis.");

            var context = localPluginContext.PluginExecutionContext;

            // Get the current record
            var mergedEntity = localPluginContext.CurrentEntity;
            tracingService.Trace("Retrieved CurrentEntity with Id: {0}", mergedEntity.Id);

            // Check if there's a summarized answer
            var answer = mergedEntity.GetAttributeValue<string>("mf_summarizedanswer");
            var question = mergedEntity.GetAttributeValue<string>("mf_questiontext");
            tracingService.Trace("mf_summarizedanswer present: {0}, mf_questiontext present: {1}",
                !String.IsNullOrWhiteSpace(answer), !String.IsNullOrWhiteSpace(question));

            if (!String.IsNullOrWhiteSpace(answer) && !String.IsNullOrWhiteSpace(question))
            {
                tracingService.Trace("Both summarized answer and question text are present. Preparing instruction and data for AI service.");

                var instruction = "You are to come up with a series of colors for a smart light bulb that represents the answer " +
                    "to the given question. Kelvin must be between 2700 and 6500. To provide your answer, please only return a JSON array of objects with the following " +
                    " properties: {\r\n        public string Hex { get; set; }\r\n        public Int32? Hue { get; set; }\r\n        public Int32? Saturation { get; set; }\r\n        public Int32? Brightness { get; set; }\r\n        public Int32? Kelvin { get; set; }\r\n        public int DurationMs { get; set; } = 1000; // Default 1 second\r\n    }";
                var data = $"Question: {question} || Answer: {answer}";

                // Send the answers to the AI service
                tracingService.Trace("Initializing ApimClient.");
                ApimClient client = new ApimClient(
                    localPluginContext.EnvironmentVariableService,
                    EnvironmentVariableService.ApimTokenCredentials,
                    EnvironmentVariableService.ApimSubscriptionDetails);

                var message = new
                {
                    Instruction = instruction,
                    Data = data,
                };

                tracingService.Trace("Sending request to AI service (DispatchChatSummary).");
                var result = client.PostAsync("DispatchChatSummary", message).Result;
                tracingService.Trace("Received response from AI service: {0}", result);

                var obj = JObject.Parse(result);
                string innerResponse = obj["response"].ToString();
                tracingService.Trace("Extracted inner response from AI service.");

                var bulbControlRequest = new BulbControlRequest()
                {
                    Colors = JsonConvert.DeserializeObject<List<ColorStep>>(innerResponse),
                    Pattern = new PatternSettings()
                    {
                        Type = PatternType.Sequential,
                        RepeatCount = 3,
                        Transition = TransitionType.Fade,
                        TransitionDurationMs = 500
                    }
                };

                var bulbPayload = Newtonsoft.Json.JsonConvert.SerializeObject(bulbControlRequest);
                var bulbRequest = new OrganizationRequest("mf_BulbApi");
                bulbRequest.Parameters.Add("BulbApiPayload", bulbPayload);
                localPluginContext.PluginUserService.Execute(bulbRequest);
                localPluginContext.Trace("Color Analysis Execution Completed");

            }
            else
            {
                tracingService.Trace("Either summarized answer or question text is missing. No action taken.");
            }

            tracingService.Trace("Exiting ExecuteDataversePlugin for QuestionColorAnalysis.");
        }
    }
}
