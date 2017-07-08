using MeetingRoomManagerLUIS.Common;
using MeetingRoomManagerLUIS.HttpWrapper;
using MeetingRoomManagerLUIS.Models;
using MeetingRoomManagerLUIS.Output;
using MeetingRoomManagerLUIS.Services;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            //var result = new ValidateResult { IsValid = false, Value = typedText };
            //result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now.AddMinutes(-15));
            //result.Feedback = result.IsValid ? string.Empty : $"{typedText} must be greater than current date.";
            //return Task.FromResult(result);
            return Task.FromResult(new ValidateResult { IsValid = true, Value = typedText });
        }

        public static Task<ValidateResult> ValidateEnd(ScheduleInformation state, object response)
        {
            DateTime typedText = (DateTime)response;
            //var result = new ValidateResult { IsValid = false, Value = typedText };
            //result.IsValid = (!typedText.Date.Equals(DateTime.MinValue.Date) && typedText > DateTime.Now && typedText > state.Start);
            //result.Feedback = result.IsValid ? string.Empty : $"{typedText} must be greater than current or start date.";
            //return Task.FromResult(result);
            return Task.FromResult(new ValidateResult { IsValid = true, Value = typedText });
        }

        private static bool ValidateLocation(string location)
        {
            string errorMessage = string.Empty;
            if (!location.Equals("Temp data"))
            {
                HttpResponseMessage Response = new HttpCalls().Get(UrlConstants.GetRooms, out errorMessage);
                if (Response.IsSuccessStatusCode)
                {
                    List<Rooms> lstRooms = new MRBSDataServices().GetRooms(out errorMessage);
                    return (lstRooms != null && lstRooms.Any(c => c.ROOM_NAME.Equals(location, StringComparison.OrdinalIgnoreCase)));
                }
            }
            else
            {
                return true;
            }
            return false;
        }
    }
}