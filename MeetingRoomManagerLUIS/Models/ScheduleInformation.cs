using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingRoomManagerLUIS.Models
{
    [Serializable]
    public class ScheduleInformation
    {
        [Prompt("What is your {&}? {||}")]
        [Describe("Employee Id")]
        public string EmployeeId { get; set; }

        [Prompt("Where you would like to book at (Type {&})? {||}")]
        [Describe("Conference room")]
        public string Location { get; set; }

        [Prompt("Enter meeting start time (MM/dd/yyyy HH:mm)")]
        public DateTime? Start { get; set; }

        [Prompt("Enter meeting end time (MM/dd/yyyy HH:mm)")]
        public DateTime? End { get; set; }

        [Prompt("Enter meeting subject")]
        public string Subject { get; set; }
    }
}