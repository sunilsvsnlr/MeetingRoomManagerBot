using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Output
{
    public class AvailableMeetingRoomInfo
    {
        public string MeetingRoomName { get; set; }
        public string RoomId { get; set; }
        public string Subject { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
}