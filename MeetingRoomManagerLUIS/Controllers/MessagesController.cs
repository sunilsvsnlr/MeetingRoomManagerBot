using MeetingRoomManagerLUIS.Dialogs;
using MeetingRoomManagerLUIS.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MeetingRoomManagerLUIS.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity != null)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, MakeRoot);
                        break;
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private static IForm<ScheduleInformation> BuildForm()
        {
            var builder = new FormBuilder<ScheduleInformation>();
            return builder
                .AddRemainingFields()
                .Field("Location", validate: ValidateLocation)
                .Field("Start", validate: ValidateStart)
                .Field("End", validate: ValidateEnd)
                .Build();
        }

        internal static IDialog<ScheduleInformation> MakeRoot()
        {
            return Chain.From(() => new CreateSchedulerDialog(BuildForm));
        }

        private static Task<ValidateResult> ValidateLocation(ScheduleInformation state, object response)
        {
            string typedText = (string)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!string.IsNullOrEmpty(typedText) && ValidateLocation(typedText));
            result.Feedback = result.IsValid ? string.Empty : $"{typedText} is not a conference room option.";
            return Task.FromResult(result);
        }

        private static Task<ValidateResult> ValidateStart(ScheduleInformation state, object response)
        {
            DateTime typedText = (DateTime)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now.AddMinutes(-15));
            result.Feedback = result.IsValid ? string.Empty : $"{typedText} must be greater than current date.";
            return Task.FromResult(result);
        }

        private static Task<ValidateResult> ValidateEnd(ScheduleInformation state, object response)
        {
            DateTime typedText = (DateTime)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now && typedText > state.Start);
            result.Feedback = result.IsValid ? string.Empty :$"{typedText} must be greater than current or start date.";
            return Task.FromResult(result);
        }

        private static bool ValidateLocation(string location)
        {
            return (location.Equals("conf1", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf2", StringComparison.InvariantCultureIgnoreCase)
                || location.Equals("conf3", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf4", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
