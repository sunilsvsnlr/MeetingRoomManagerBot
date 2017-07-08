using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace MeetingRoomManagerLUIS.Common
{
    public static class Utilities
    {
        public static string ServerUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ServerUrl"];
            }
        }

        public static string HostedService
        {
            get
            {
                return ConfigurationManager.AppSettings["HostedService"];
            }
        }
    }
}