using MeetingRoomManagerLUIS.Common;
using MeetingRoomManagerLUIS.HttpWrapper;
using MeetingRoomManagerLUIS.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MeetingRoomManagerLUIS.Services
{
    public class MRBSDataServices
    {
        public List<Rooms> GetRooms(out string errorMessage)
        {
            errorMessage = string.Empty;
            HttpResponseMessage response = new HttpCalls().Get(UrlConstants.GetRooms, out errorMessage);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<List<Rooms>>().Result;
            }
            else
            {
                errorMessage = response.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        public int GetRoomId(string location)
        {
            string errorMessage = string.Empty;
            List<Rooms> lstRooms = GetRooms(out errorMessage);
            Rooms selectedRoom = lstRooms.FirstOrDefault(c => c.ROOM_NAME.Equals(location, StringComparison.OrdinalIgnoreCase));
            if (selectedRoom != null)
            {
                return selectedRoom.ID;
            }
            return 0;
        }
    }
}