using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AlexaSkill.Data;
using Microsoft.Ajax.Utilities;

namespace AlexaSkill.Controllers
{
    public class AlexaController : ApiController
    {
        [HttpPost, Route("api/alexa/demo")]
        public AlexaResponse Namely(AlexaRequest alexaRequest)
        {
            var request = new Requests().Create(new Data.Request
            {
                MemberId = alexaRequest.Session.Attributes?.MemberId ?? 0,
                Timestamp = alexaRequest.Request.Timestamp,
                Intent = (alexaRequest.Request.Intent == null) ? "" : alexaRequest.Request.Intent.Name,
                AppId = alexaRequest.Session.Application.ApplicationId,
                RequestId = alexaRequest.Request.RequestId,
                SessionId = alexaRequest.Session.SessionId,
                UserId = alexaRequest.Session.User.UserId,
                IsNew = alexaRequest.Session.New,
                Version = alexaRequest.Version,
                Type = alexaRequest.Request.Type,
                Reason = alexaRequest.Request.Reason ?? "",
                SlotsList = alexaRequest.Request.Intent.GetSlots(),
                DateCreated = DateTime.UtcNow
            });

            AlexaResponse response = null;

            switch (request.Type)
            {
                case "LaunchRequest":
                    response = LaunchRequestHandler(request);
                    break;
                case "IntentRequest":
                    response = IntentRequestHandler(request);
                    break;
                case "SessionEndedRequest":
                    response = SessionEndedRequestHandler(request);
                    break;
            }


            return response;



        }

        private AlexaResponse LaunchRequestHandler(Request request)
        {
            var response = new AlexaResponse("Welcome to Namely. What would you like to hear?")
            {
                Session = { MemberId = request.MemberId }
            };
            response.Response.Card.Title = "Namely";
            response.Response.Card.Content = "Say HR";
            response.Response.Reprompt.OutputSpeech.Text = "Please pick one, New Hires orCompany Holidays?";
            response.Response.ShouldEndSession = false;

            if (request.Intent == "AMAZON.NoIntent")
            {
                response.Response.OutputSpeech.Text = "Ok, have a nice day.";
                response.Response.ShouldEndSession = true;
            }

            return response;
        }

        private AlexaResponse IntentRequestHandler(Request request)
        {
            AlexaResponse response = null;

            switch (request.Intent)
            {
                case "CompanyHolidayIntent":
                    response = CompanyHolidayIntentHandler(request);
                    break;
                case "WhosOutIntent":
                    response = WhosOutIntentHandler(request);
                    break;
                case "NewHiresIntent":
                    response = NewHiresIntentHandler(request);
                    break;
                case "AMAZON.CancelIntent":
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntentHandler(request);
                    break;
            }
            return response;

        }

        private AlexaResponse WhosOutIntentHandler(Request request)
        {
            var output = new StringBuilder();
            var endsession = true;
            if (request.SlotsList.Any())
            {
                var firstName = request.SlotsList.FirstOrDefault(s => s.Key == "FirstName").Value;
                var lastName = request.SlotsList.FirstOrDefault(s => s.Key == "LastName").Value;
                if (firstName != string.Empty)
                {
                    firstName = firstName.ToLower();
                    switch (firstName)
                    {
                        case "arnold":
                            output.Append($"{firstName} {lastName} is working from home today.");
                            break;
                        case "daniel":
                            output.Append($"{firstName} {lastName} should be in the office today.");
                            break;
                        default:
                            output.AppendLine($"I'm not sure who {firstName} {lastName} is. Please try again.");
                            break;
                    }
                }
                else
                {
                    output.AppendLine("I didn't get the name. Try again.");
                }
            }
            else
            {
                output.Append("Tell me who you would like to check on.");
                endsession = false;
            }

            return new AlexaResponse(output.ToString(), endsession);
        }

        private AlexaResponse HelpIntentHandler(Request request)
        {
            var response = new AlexaResponse("You can ask Namely to get a New Hires list or information about your company holidays.", false);
            response.Response.Reprompt.OutputSpeech.Text = "How about asking about New Hires or your company holidays?";
            return response;
        }

        private AlexaResponse CancelOrStopIntentHandler(Request request)
        {
            return new AlexaResponse("Thanks for listening, let's talk again soon.", true);
        }

        private AlexaResponse CompanyHolidayIntentHandler(Request request)
        {
            var output = new StringBuilder();
            var endsession = true;
            if (request.SlotsList.Any())
            {
                var holidayCriteria = request.SlotsList.FirstOrDefault(s => s.Key == "Holiday").Value;
                switch (holidayCriteria)
                {
                    case "last":
                        output.Append($"Your {holidayCriteria} company holiday, Labor Day, was on September 5th.");
                        break;
                    case "next":
                        output.Append($"Your {holidayCriteria} company holiday, Thanksgiving, will be on November 24th.");
                        break;
                }


            }
            else
            {
                output.Append("Would you like to know when your next company holiday will be? Say yes, or no.");
                endsession = false;
            }

            return new AlexaResponse(output.ToString(), endsession);
        }

        private AlexaResponse NewHiresIntentHandler(Request request)
        {
            int limit = 10;
            var criteria = string.Empty;

            if (request.SlotsList.Any())
            {
                int maxLimit = 10;
                var limitValue = request.SlotsList.FirstOrDefault(s => s.Key == "Limit").Value;

                if (!string.IsNullOrWhiteSpace(limitValue) && int.TryParse(limitValue, out limit) && !(limit >= 1 && limit <= maxLimit))
                {
                    limit = maxLimit;
                }

                criteria = request.SlotsList.FirstOrDefault(s => s.Key == "Criteria").Value;
            }

            var output = new StringBuilder();

            criteria = string.IsNullOrWhiteSpace(criteria) ? "hires" : criteria;

            output.Append($"Here are the top {limit} {criteria}. ");

            switch (criteria)
            {
                case "hires":
                    output.AppendLine("Arnold Shortman");
                    output.AppendLine("Gerald Johanssen");
                    output.AppendLine("Helga Crabtree");
                    output.AppendLine("Oscar Kokoshka");
                    output.AppendLine("Ernie Potts");
                    break;
                case "employees":
                    output.AppendLine("Arnold Shortman");
                    output.AppendLine("Gerald Johanssen");
                    output.AppendLine("Helga Crabtree");
                    break;
                case "contractors":
                    output.AppendLine("Oscar Kokoshka");
                    output.AppendLine("Ernie Potts");
                    break;
            }



            return new AlexaResponse(output.ToString());
        }

        private AlexaResponse SessionEndedRequestHandler(Request request)
        {
            return null;
        }




    }

}
