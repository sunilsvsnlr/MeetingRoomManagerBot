using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Common
{
    public static class UrlConstants
    {
        public static string ValidateLogin = "api/LoginApi?userName={0}&password={1}";
        public static string GetRooms = "api/roomsApi";
        public static string BookConferenceRoom = "api/BookingRoomApi";
        public static string GetLoggedInEmployeeSchedules = "api/GetRoomsByEmployee?userName={0}";
        public static string GetLoggedInEmployeeRooms = "api/meetingroomapi?userName={0}&startDate={1}&endDate={2}";
        public static string GetSelectedRooms = "api/meetingroomapi?userName={0}&room={1}&startDate={2}&endDate={3}";
        public static string CancelSchedule = "api/cancel?CreatedBy={0}&RoomId={1}&StartTime={2}&EndTime={3}";
    }
}