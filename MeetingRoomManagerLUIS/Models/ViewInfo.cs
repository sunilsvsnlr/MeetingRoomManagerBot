using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Models
{
    [Serializable]
    public class ViewInfo
    {
        public string Location { get; set; }
       
        public string BookingId { get; set; }
        
        public string Owner { get; set; }
    }
}