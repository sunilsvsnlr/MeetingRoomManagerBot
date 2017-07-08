using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Extensions
{
    public static class ViewInfoValidations
    {
        public static bool ValidateRoomName(string location)
        {
            return (location.Equals("conf1", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf2", StringComparison.InvariantCultureIgnoreCase)
                || location.Equals("conf3", StringComparison.InvariantCultureIgnoreCase) || location.Equals("conf4", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}