using MeetingRoomManagerLUIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.ServiceInputs
{
    public class CreateSchedulerInformation
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public int RoomId { get; set; }
        public string Subject { get; set; }
    }
}