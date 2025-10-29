using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
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
    public class QuestionSentimentSummary : PluginBase
    {
        public QuestionSentimentSummary(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(QuestionSentimentSummary))
        {
        }
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;
            tracingService.Trace("Entered ExecuteDataversePlugin for QuestionSentimentSummary.");

            var context = localPluginContext.PluginExecutionContext;

            // Get the current record
            var mergedEntity = localPluginContext.CurrentEntity;
            tracingService.Trace("Retrieved CurrentEntity with Id: {0}", mergedEntity.Id);

            // Drop in here if the question status is Closed and there are Summary Instructions
            var questionStatus = mergedEntity.GetAttributeValue<OptionSetValue>("statuscode");
            var summaryInstructions = mergedEntity.GetAttributeValue<string>("mf_summarizationinstructions");
            tracingService.Trace("Question status: {0}, Summary instructions present: {1}",
                questionStatus?.Value, !String.IsNullOrWhiteSpace(summaryInstructions));

            if (questionStatus != null && questionStatus.Value == 100000001 && !String.IsNullOrWhiteSpace(summaryInstructions))
            {
                tracingService.Trace("Question is closed and summary instructions are present. Retrieving answers.");

                // Retrieve the answers
                var allAnswers = RetrieveAnswersForTheQuestion(localPluginContext.InitiatingUserService, mergedEntity.Id);
                tracingService.Trace("Retrieved {0} answers for question Id: {1}", allAnswers.Count, mergedEntity.Id);

                // Send the answers to the AI service
                tracingService.Trace("Initializing ApimClient.");
                ApimClient client = new ApimClient(
                    localPluginContext.EnvironmentVariableService,
                    EnvironmentVariableService.ApimTokenCredentials,
                    EnvironmentVariableService.ApimSubscriptionDetails);

                // Prime the data to send in a single string value
                summaryInstructions += ". The list of input values are separated by a | character.";

                var message = new
                {
                    Instruction = summaryInstructions,
                    Data = String.Join("|", allAnswers),
                };

                tracingService.Trace("Sending data to AI service for summarization.");
                var result = client.PostAsync("DispatchChatSummary", message).Result;

                var obj = JObject.Parse(result);
                string innerResponse = obj["response"].ToString();

                tracingService.Trace("Received summary result from AI service.");

                // Update the question record with the summary
                localPluginContext.TargetEntity["mf_summarizedanswer"] = innerResponse;
                tracingService.Trace("Updated TargetEntity with summarized answer.");
            }
            else
            {
                tracingService.Trace("Question is not closed or summary instructions are missing. No action taken.");
            }

            tracingService.Trace("Exiting ExecuteDataversePlugin for QuestionSentimentSummary.");
        }

        protected List<string> RetrieveAnswersForTheQuestion(IOrganizationService service, Guid questionId)
        {
            QueryExpression qe = new QueryExpression("mf_answer");
            qe.ColumnSet = new ColumnSet("mf_answertext");
            qe.Criteria.AddCondition("mf_question", ConditionOperator.Equal, questionId);
            qe.Criteria.AddCondition("mf_answertext", ConditionOperator.NotNull);
            var answerRecords = service.RetrieveMultiple(qe).Entities.ToList();
            return answerRecords.Select(a => a.GetAttributeValue<string>("mf_answertext")).ToList();
        }
    }
}
