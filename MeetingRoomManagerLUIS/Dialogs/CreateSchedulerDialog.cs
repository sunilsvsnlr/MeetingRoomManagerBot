using AdaptiveCards;
using MeetingRoomManagerLUIS.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MeetingRoomManagerLUIS.Dialogs
{
    [LuisModel("7d838588-89ab-4cb4-92f9-e8863918bc95", "d1b7382449a8439cb46e7a60ff846e6b", LuisApiVersion.V2)]
    [Serializable]
    public class CreateSchedulerDialog : LuisDialog<ScheduleInformation>
    {
        private readonly BuildFormDelegate<ScheduleInformation> MakeScheduleForm;
        internal CreateSchedulerDialog(BuildFormDelegate<ScheduleInformation> makeScheduleForm)
        {
            this.MakeScheduleForm = makeScheduleForm;
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
                            entityRecommendation.Add(new EntityRecommendation() { Type = "Location", Entity = scheduleInfo.Location });
                            break;
                        case "Subject":
                            scheduleInfo.Subject = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = "Subject", Entity = scheduleInfo.Subject });
                            break;
                        case "builtin.datetimeV2.datetime":
                        case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                scheduleInfo.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                            }
                            break;
                        default:
                            break;
                    }
                });

                if (duration > 0)
                {
                    scheduleInfo.Start = TimeZone.CurrentTimeZone.ToLocalTime((scheduleInfo.Start == DateTime.MinValue) ? DateTime.Now : scheduleInfo.Start);
                    AddOrUpdateStartEntity(entityRecommendation, scheduleInfo);
                    scheduleInfo.End = TimeZone.CurrentTimeZone.ToLocalTime(duration > 0 ? scheduleInfo.Start.AddSeconds(duration) : scheduleInfo.End);
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

        private static void AddOrUpdateStartEntity(List<EntityRecommendation> entityRecommendation, ScheduleInformation scheduleInfo)
        {
            if (!entityRecommendation.Any(c => c.Type.Equals("Start")))
            {
                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
            }
            else
            {
                EntityRecommendation startEntity = entityRecommendation.Find(c => c.Type.Equals("Start"));
                startEntity.Entity = scheduleInfo.Start.ToString();
            }
        }

        private async Task CompleteCreateSchedule(IDialogContext context, IAwaitable<ScheduleInformation> result)
        {
            ScheduleInformation scheduleInfo = null;
            try
            {
                scheduleInfo = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("Form cancelled.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }

            if (scheduleInfo != null)
            {
                #region Adaptive Card to get rich output
                IMessageActivity message = context.MakeMessage();
                message.Attachments = new List<Attachment>();

                AdaptiveCard card = new AdaptiveCard();
                card.Body.Add(new TextBlock()
                {
                    Text = "Thanks for submitting...",
                    Wrap = true,
                    Size = TextSize.ExtraLarge,
                    Weight = TextWeight.Bolder
                });

                card.Body.Add(new TextBlock()
                {
                    Text = "Below are the details are under process...",
                    Wrap = true,
                    Size = TextSize.Large,
                    Weight = TextWeight.Bolder
                });

                card.Body.Add(new TextBlock() { Text = $"Employee Id: {scheduleInfo.EmployeeId}", Weight = TextWeight.Normal });
                card.Body.Add(new TextBlock() { Text = $"Subject: {scheduleInfo.Subject}", Weight = TextWeight.Normal });
                card.Body.Add(new TextBlock() { Text = $"Location: {scheduleInfo.Location}", Weight = TextWeight.Normal });
                card.Body.Add(new TextBlock() { Text = $"Start: {scheduleInfo.Start.ToString("dd-MMM-yyyy hh:mm tt")}", Weight = TextWeight.Normal });
                card.Body.Add(new TextBlock() { Text = $"End: {scheduleInfo.End.ToString("dd-MMM-yyyy hh:mm tt")}", Weight = TextWeight.Normal });

                message.Attachments.Add(new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card }); 
                #endregion
                await context.PostAsync(message);
            }
            //context.Wait(MessageReceived);
            context.Done<ScheduleInformation>(scheduleInfo);
        }
    }
}