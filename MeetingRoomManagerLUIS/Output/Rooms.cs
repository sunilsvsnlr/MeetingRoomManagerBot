using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Output
{
    public class Rooms
    {
        public int ID { get; set; }
        public int AREA_ID { get; set; }
        public string ROOM_NAME { get; set; }
        public string SORT_KEY { get; set; }
        public string DESCRIPTION { get; set; }
        public string CAPACITY { get; set; }
    }
}