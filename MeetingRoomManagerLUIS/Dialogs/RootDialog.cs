using MeetingRoomManagerLUIS.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MeetingRoomManagerLUIS.Extensions;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Threading;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using MeetingRoomManagerLUIS.Output;
using MeetingRoomManagerLUIS.Services;

namespace MeetingRoomManagerLUIS.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(
               context,
               this.AfterChoiceSelected,
               new[] { "MeetingRoomManager", "EmployeeLeaves" },
               "What service would like to select?",
               "I am sorry but I didn't understand that. Please select one of the options.",
               attempts: 2);
        }

        private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var selection = await result;

                switch (selection)
                {
                    case "EmployeeLeaves":
                        await context.PostAsync("This functionality is not yet implemented!");
                        await this.StartAsync(context);
                        break;

                    case "MeetingRoomManager":
                        await context.PostAsync("Welcome to meeting room manager.");
                        context.Forward(new CreateSchedulerDialog(BuildForm, BuildViewForm), this.ResumeScheduler, context.Activity, CancellationToken.None);
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                await this.StartAsync(context);
            }
        }

        private async Task ResumeScheduler(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private static IForm<ScheduleInformation> BuildForm()
        {
            var builder = new FormBuilder<ScheduleInformation>();
            return builder
                .AddRemainingFields()
                 //.Field(new FieldReflector<ScheduleInformation>("Location")
                 //   .SetType(null)
                 //   .SetValidate(ScheduleInfoValidations.ValidateLocation)
                 //   .SetDefine((state, field) =>
                 //   {
                 //       string errorMessage = string.Empty;
                 //       List<Rooms> lstRooms = new MRBSDataServices().GetRooms(out errorMessage);
                 //       lstRooms.ForEach(c =>
                 //       {
                 //           field.AddDescription(c.SORT_KEY, c.SORT_KEY).AddTerms(c.SORT_KEY, c.SORT_KEY);
                 //       });
                 //       return Task.FromResult(true);
                 //   }))
                .Field("Location", validate: ScheduleInfoValidations.ValidateLocation)
                .Field("Start", validate: ScheduleInfoValidations.ValidateStart)
                .Field("End", validate: ScheduleInfoValidations.ValidateEnd)
                .Build();
        }

        private static IForm<ViewInfo> BuildViewForm()
        {
            var builder = new FormBuilder<ViewInfo>();
            return builder
                .AddRemainingFields()
                .Build();
        }

        internal static IDialog<ScheduleInformation> MakeRoot()
        {
            return Chain.From(() => new CreateSchedulerDialog(BuildForm, BuildViewForm));
        }
    }
}