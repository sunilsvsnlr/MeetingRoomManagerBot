using MeetingRoomManagerLUIS.Common;
using MeetingRoomManagerLUIS.HttpWrapper;
using MeetingRoomManagerLUIS.Models;
using MeetingRoomManagerLUIS.Output;
using MeetingRoomManagerLUIS.ServiceInputs;
using MeetingRoomManagerLUIS.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MeetingRoomManagerLUIS.Dialogs
{
    [LuisModel("7d838588-89ab-4cb4-92f9-e8863918bc95", "d1b7382449a8439cb46e7a60ff846e6b", LuisApiVersion.V2)]
    [Serializable]
    public class CreateSchedulerDialog : LuisDialog<ScheduleInformation>
    {
        private readonly BuildFormDelegate<ScheduleInformation> MakeScheduleForm;
        private readonly BuildFormDelegate<ViewInfo> MakeViewForm;

        internal CreateSchedulerDialog(BuildFormDelegate<ScheduleInformation> makeScheduleForm, BuildFormDelegate<ViewInfo> makeViewForm)
        {
            this.MakeScheduleForm = makeScheduleForm;
            this.MakeViewForm = makeViewForm;
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Booking Room")]
        public async Task ProcessSchedulerForm(IDialogContext context, LuisResult result)
        {
            if (result.Entities != null && result.Entities.Count > 0)
            {
                List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
                ScheduleInformation scheduleInfo = new ScheduleInformation();
                long duration = 0;
                result.Entities.ToList().ForEach(c =>
                {
                    switch (c.Type)
                    {
                        case "Location":
                            scheduleInfo.Location = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Location });
                            break;
                        case "Subject":
                            scheduleInfo.Subject = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Subject });
                            break;
                        case "builtin.datetimeV2.datetime":
                        case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                scheduleInfo.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Start.ToString() });
                                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = duration.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.timerange":
                        case "builtin.datetimeV2.datetimerange":
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start") != null
                            || ((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end") != null)
                            {
                                DateTime startRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start");
                                DateTime endRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end");

                                if (c.Type.Equals("builtin.datetimeV2.datetimerange"))
                                {
                                    scheduleInfo.Start = startRange;
                                    scheduleInfo.End = endRange;
                                }
                                else
                                {
                                    scheduleInfo.Start = scheduleInfo.Start.HasValue ? scheduleInfo.Start : DateTime.Now;
                                    scheduleInfo.End = scheduleInfo.End.HasValue ? scheduleInfo.End : DateTime.Now;

                                    scheduleInfo.Start = new DateTime(scheduleInfo.Start.Value.Year, scheduleInfo.Start.Value.Month, scheduleInfo.Start.Value.Day, startRange.Hour, startRange.Minute, startRange.Second);
                                    scheduleInfo.End = new DateTime(scheduleInfo.End.Value.Year, scheduleInfo.End.Value.Month, scheduleInfo.End.Value.Day, endRange.Hour, endRange.Minute, endRange.Second);
                                }

                                AddOrUpdateEntity(entityRecommendation, "Start", scheduleInfo.Start.ToString());
                                AddOrUpdateEntity(entityRecommendation, "End", scheduleInfo.End.ToString());
                            }
                            break;
                        default:
                            break;
                    }
                });

                if (duration > 0)
                {
                    scheduleInfo.Start = TimeZone.CurrentTimeZone.ToLocalTime(scheduleInfo.Start.HasValue ? DateTime.Now : scheduleInfo.Start.Value);
                    AddOrUpdateEntity(entityRecommendation, "Start", scheduleInfo.Start.ToString());
                    scheduleInfo.End = TimeZone.CurrentTimeZone.ToLocalTime(duration > 0 ? scheduleInfo.Start.Value.AddSeconds(duration) : scheduleInfo.End.Value);
                    entityRecommendation.Add(new EntityRecommendation() { Type = "End", Entity = scheduleInfo.End.ToString() });
                }


                var createscheduleform = new FormDialog<ScheduleInformation>(scheduleInfo, this.MakeScheduleForm, FormOptions.PromptInStart, entityRecommendation);
                context.Call<ScheduleInformation>(createscheduleform, CompleteCreateSchedule);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }
        }

        [LuisIntent("Show Rooms")]
        public async Task ProcessViewForm(IDialogContext context, LuisResult result)
        {
            List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
            long duration = 0;
            ScheduleInformation scheduleInfo = new ScheduleInformation();

            result.Entities.ToList().ForEach(c =>
            {
                switch (c.Type)
                {
                    case "Location":
                        scheduleInfo.Location = c.Entity;
                        entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Location });
                        break;
                    case "builtin.datetimeV2.datetime":
                    case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                        {
                            scheduleInfo.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Start.ToString() });
                            entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                        }
                        break;
                    case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                        {
                            duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = duration.ToString() });
                        }
                        break;
                    case "builtin.datetimeV2.timerange":
                    case "builtin.datetimeV2.datetimerange":
                        if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start") != null
                        || ((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end") != null)
                        {
                            DateTime startRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start");
                            DateTime endRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end");

                            if (c.Type.Equals("builtin.datetimeV2.datetimerange"))
                            {
                                scheduleInfo.Start = startRange;
                                scheduleInfo.End = endRange;
                            }
                            else
                            {
                                scheduleInfo.Start = scheduleInfo.Start.HasValue ? scheduleInfo.Start : DateTime.Now;
                                scheduleInfo.End = scheduleInfo.End.HasValue ? scheduleInfo.End : DateTime.Now;

                                scheduleInfo.Start = new DateTime(scheduleInfo.Start.Value.Year, scheduleInfo.Start.Value.Month, scheduleInfo.Start.Value.Day, startRange.Hour, startRange.Minute, startRange.Second);
                                scheduleInfo.End = new DateTime(scheduleInfo.End.Value.Year, scheduleInfo.End.Value.Month, scheduleInfo.End.Value.Day, endRange.Hour, endRange.Minute, endRange.Second);
                            }

                            AddOrUpdateEntity(entityRecommendation, "Start", scheduleInfo.Start.ToString());
                            AddOrUpdateEntity(entityRecommendation, "End", scheduleInfo.End.ToString());
                        }
                        break;
                    default:
                        break;
                }
            });

            scheduleInfo.Start = scheduleInfo.Start.HasValue ? scheduleInfo.Start : DateTime.Now.AddHours(1);
            scheduleInfo.End = scheduleInfo.End.HasValue ? scheduleInfo.End : DateTime.Now.AddDays(1);
            scheduleInfo.Subject = "Temp Subject";
            scheduleInfo.Location = "Temp data";

            var showSchedules = new FormDialog<ScheduleInformation>(scheduleInfo, this.MakeScheduleForm, FormOptions.PromptInStart, entityRecommendation);
            context.Call<ScheduleInformation>(showSchedules, ShowRooms);
        }

        [LuisIntent("Cancel Room")]
        public async Task CancelSchedulerForm(IDialogContext context, LuisResult result)
        {
            if (result.Entities != null && result.Entities.Count > 0)
            {
                List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
                long duration = 0;
                ScheduleInformation scheduleInfo = new ScheduleInformation();

                result.Entities.ToList().ForEach(c =>
                {
                    switch (c.Type)
                    {
                        case "Location":
                            scheduleInfo.Location = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Location });
                            break;
                        case "builtin.datetimeV2.datetime":
                        case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                scheduleInfo.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Start.ToString() });
                                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = duration.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.timerange":
                        case "builtin.datetimeV2.datetimerange":
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start") != null
                            || ((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end") != null)
                            {
                                DateTime startRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("start");
                                DateTime endRange = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("end");

                                if (c.Type.Equals("builtin.datetimeV2.datetimerange"))
                                {
                                    scheduleInfo.Start = startRange;
                                    scheduleInfo.End = endRange;
                                }
                                else
                                {
                                    scheduleInfo.Start = scheduleInfo.Start.HasValue ? scheduleInfo.Start : DateTime.Now;
                                    scheduleInfo.End = scheduleInfo.End.HasValue ? scheduleInfo.End : DateTime.Now;

                                    scheduleInfo.Start = new DateTime(scheduleInfo.Start.Value.Year, scheduleInfo.Start.Value.Month, scheduleInfo.Start.Value.Day, startRange.Hour, startRange.Minute, startRange.Second);
                                    scheduleInfo.End = new DateTime(scheduleInfo.End.Value.Year, scheduleInfo.End.Value.Month, scheduleInfo.End.Value.Day, endRange.Hour, endRange.Minute, endRange.Second);
                                }

                                AddOrUpdateEntity(entityRecommendation, "Start", scheduleInfo.Start.ToString());
                                AddOrUpdateEntity(entityRecommendation, "End", scheduleInfo.End.ToString());
                            }
                            break;
                        default:
                            break;
                    }
                });

                scheduleInfo.Subject = "Temp Subject";

                var cancelAppointment = new FormDialog<ScheduleInformation>(scheduleInfo, this.MakeScheduleForm, FormOptions.PromptInStart, entityRecommendation);
                context.Call<ScheduleInformation>(cancelAppointment, cancelMeeting);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }
        }

        private static void AddOrUpdateEntity(List<EntityRecommendation> entityRecommendation, string entityType, string value)
        {
            if (!entityRecommendation.Any(c => c.Type.Equals(entityType)))
            {
                entityRecommendation.Add(new EntityRecommendation() { Type = entityType, Entity = value });
            }
            else
            {
                EntityRecommendation startEntity = entityRecommendation.Find(c => c.Type.Equals("Start"));
                startEntity.Entity = value;
            }
        }

        private async Task cancelMeeting(IDialogContext context, IAwaitable<ScheduleInformation> result)
        {
            HttpCalls httpcalls = new HttpCalls();
            ScheduleInformation scheduleInfo = null;
            string errorMessage = string.Empty;
            try
            {
                scheduleInfo = await result;
            }
            catch (OperationCanceledException ex)
            {
                await context.PostAsync(ex.Message);
                await context.PostAsync("Form cancelled.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }

            if (scheduleInfo != null)
            {
                HttpResponseMessage response = null;
                string query = string.Format(CultureInfo.CurrentCulture,
                    UrlConstants.CancelSchedule, scheduleInfo.UserName, new MRBSDataServices().GetRoomId(scheduleInfo.Location), scheduleInfo.Start.Value, scheduleInfo.End.Value);
                response = new HttpCalls().Delete(query, out errorMessage);

                #region Adaptive Card to get rich output
                IMessageActivity message = context.MakeMessage();
                message.Attachments = new List<Attachment>();
                #endregion

                if (response.IsSuccessStatusCode)
                {
                    message.Attachments.Add((new HeroCard()
                    {
                        Title = $"Cancelled Successfully.",
                        Text = string.Empty
                    }).ToAttachment());
                    await context.PostAsync(message);
                    context.Done<ScheduleInformation>(scheduleInfo);
                }
                else
                {
                    await context.PostAsync(response.Content.ReadAsStringAsync().Result);
                    context.Wait(MessageReceived);
                }
            }
        }

        private async Task ShowRooms(IDialogContext context, IAwaitable<ScheduleInformation> result)
        {
            HttpCalls httpcalls = new HttpCalls();
            ScheduleInformation scheduleInfo = null;
            string errorMessage = string.Empty;
            try
            {
                scheduleInfo = await result;
            }
            catch (OperationCanceledException ex)
            {
                await context.PostAsync(ex.Message);
                await context.PostAsync("Form cancelled.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }

            if (scheduleInfo != null)
            {
                HttpResponseMessage response = null;
                List<AvailableMeetingRoomInfo> lstShowRooms = null;
                if (string.IsNullOrEmpty(scheduleInfo.Location) || scheduleInfo.Location.Equals("Temp data"))
                {
                    string query = string.Format(CultureInfo.CurrentCulture,
                        UrlConstants.GetLoggedInEmployeeSchedules, scheduleInfo.UserName);
                    response = new HttpCalls().Get(query, out errorMessage);
                }
                else
                {
                    string query = string.Format(CultureInfo.CurrentCulture,
                        UrlConstants.GetSelectedRooms, scheduleInfo.UserName, new MRBSDataServices().GetRoomId(scheduleInfo.Location), scheduleInfo.Start.Value.Date, scheduleInfo.End.Value.Date);
                    response = new HttpCalls().Get(query, out errorMessage);
                }

                #region Adaptive Card to get rich output
                IMessageActivity message = context.MakeMessage();
                message.Attachments = new List<Attachment>();

                lstShowRooms = response.Content.ReadAsAsync<List<AvailableMeetingRoomInfo>>().Result;

                lstShowRooms.ForEach(c =>
                {
                    string cardOutput = $"Start: {(c.Start.HasValue ? c.Start.Value.ToString("dd-MMM-yyyy hh:mm tt") : string.Empty)}{Environment.NewLine}" +
                    $"End: {(c.End.HasValue ? c.End.Value.ToString("dd-MMM-yyyy hh:mm tt") : string.Empty)}";

                    message.Attachments.Add((new HeroCard()
                    {
                        Title = $"{c.Subject}",
                        Subtitle = $"{c.MeetingRoomName}",
                        Text = cardOutput
                    }).ToAttachment());
                });

                #endregion

                if (response.IsSuccessStatusCode)
                {
                    await context.PostAsync(message);
                    context.Done<ScheduleInformation>(scheduleInfo);
                }
                else
                {
                    await context.PostAsync(response.Content.ReadAsStringAsync().Result);
                    context.Wait(MessageReceived);
                }
            }
        }

        private async Task CompleteCreateSchedule(IDialogContext context, IAwaitable<ScheduleInformation> result)
        {
            HttpCalls httpcalls = new HttpCalls();
            ScheduleInformation scheduleInfo = null;
            CreateSchedulerInformation schedulerInfo = null;
            string errorMessage = string.Empty;
            try
            {
                scheduleInfo = await result;
                schedulerInfo = new CreateSchedulerInformation()
                {
                    EndDate = scheduleInfo.End.Value,
                    StartDate = scheduleInfo.Start.Value,
                    Subject = scheduleInfo.Subject,
                    CreatedBy = scheduleInfo.UserName,
                    RoomId = new MRBSDataServices().GetRoomId(scheduleInfo.Location)
                };
            }
            catch (OperationCanceledException ex)
            {
                await context.PostAsync(ex.Message);
                await context.PostAsync("Form cancelled.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }

            if (schedulerInfo != null)
            {
                HttpResponseMessage response = new HttpCalls().Post<CreateSchedulerInformation>(UrlConstants.BookConferenceRoom, schedulerInfo, out errorMessage);

                if (response.IsSuccessStatusCode)
                {
                    #region Adaptive Card to get rich output
                    IMessageActivity message = context.MakeMessage();
                    message.Attachments = new List<Attachment>();

                    string cardOutput = $"UserName: {schedulerInfo.CreatedBy}{Environment.NewLine}" +
                        $"Subject: {schedulerInfo.Subject}{Environment.NewLine}" +
                        $"Location: {scheduleInfo.Location}{Environment.NewLine}" +
                        $"Start: {schedulerInfo.StartDate.ToString("dd-MMM-yyyy hh:mm tt")}{Environment.NewLine}" +
                        $"End: {schedulerInfo.EndDate.ToString("dd-MMM-yyyy hh:mm tt")}";

                    HeroCard plCard = new HeroCard()
                    {
                        Title = $"Thanks for submitting...",
                        Subtitle = $"Schedule created successfully.",
                        Text = cardOutput
                    };
                    #endregion

                    message.Attachments.Add(plCard.ToAttachment());
                    await context.PostAsync(message);
                    context.Done<ScheduleInformation>(scheduleInfo);
                }
                else
                {
                    await context.PostAsync(response.Content.ReadAsStringAsync().Result);
                    context.Wait(MessageReceived);
                }
            }
        }
    }
}