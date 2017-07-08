using MeetingRoomManagerLUIS.Models;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MeetingRoomManagerLUIS.Extensions
{
    public static class ScheduleInfoValidations
    {
        public static Task<ValidateResult> ValidateLocation(ScheduleInformation state, object response)
        {
            string typedText = (string)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!string.IsNullOrEmpty(typedText) && ValidateLocation(typedText));
            result.Feedback = result.IsValid ? string.Empty : $"{typedText} is not a conference room option.";
            return Task.FromResult(result);
        }

        public static Task<ValidateResult> ValidateStart(ScheduleInformation state, object response)
        {
            DateTime typedText = (DateTime)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now.AddMinutes(-15));
            result.Feedback = result.IsValid ? string.Empty : $"{typedText} must be greater than current date.";
            return Task.FromResult(result);
        }

        public static Task<ValidateResult> ValidateEnd(ScheduleInformation state, object response)
        {
            DateTime typedText = (DateTime)response;
            var result = new ValidateResult { IsValid = false, Value = typedText };
            result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now && typedText > state.Start);
            result.Feedback = result.IsValid ? string.Empty : $"{typedText} must be greater than current or start date.";
            return Task.FromResult(result);
        }

        private static bool ValidateLocation(string location)
        {
            return (location.Equals("conf1", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf2", StringComparison.InvariantCultureIgnoreCase)
                || location.Equals("conf3", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf4", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}