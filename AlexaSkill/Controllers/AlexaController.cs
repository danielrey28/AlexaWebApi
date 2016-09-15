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

namespace AlexaSkill.Controllers
{
    public class AlexaController : ApiController
    {
        [HttpPost, Route("api/alexa/demo")]
        public dynamic Pluralsight(AlexaRequest alexaRequest)
        {

            try
            {
                var request = new Requests().Create(new Data.Request
                {
                    MemberId = alexaRequest.Session.Attributes.MemberId,
                    Timestamp = alexaRequest.Request.Timestamp,
                    Intent = (alexaRequest.Request.Intent == null) ? "" : alexaRequest.Request.Intent.Name,
                    AppId = alexaRequest.Session.Application.ApplicationId,
                    RequestId = alexaRequest.Request.RequestId,
                    SessionId = alexaRequest.Session.SessionId,
                    UserId = alexaRequest.Session.User.UserId,
                    IsNew = alexaRequest.Session.New,
                    Version = alexaRequest.Version,
                    Type = alexaRequest.Request.Type,
                    Reason = "",
                    DateCreated = DateTime.UtcNow,
                    Slots = ""

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
            catch (EntityCommandExecutionException ex)
            {
                return new { message = ex.Message, exceptiontype = ex.GetType(), request = alexaRequest, inner = ex.InnerException };
            }
            catch (DbEntityValidationException ex)
            {
                return new { message = ex.Message, exceptiontype = ex.GetType(), request = alexaRequest, validation = ex.EntityValidationErrors};
            }
            catch (Exception ex)
            {
                return new {message = ex.Message, exceptiontype = ex.GetType(), exceptionsource = ex.Data};
            }
        }

        private AlexaResponse LaunchRequestHandler(Request request)
        {
            var response = new AlexaResponse("Welcome to Pluralsight. What would you like to hear? Top courses or new courses?")
            {
                Session = { MemberId = request.MemberId }
            };
            response.Response.Card.Title = "Pluralsight";
            response.Response.Card.Content = "Hello\ncruel world!";
            response.Response.Reprompt.OutputSpeech.Text = "Please pick one, Top courses or new courses?";
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
                case "NewCourseIntent":
                    response = NewCoursesIntentHndler(request);
                    break;
                case "TopCoursesIntent":
                    response = TopCoursesIntentHandler(request);
                    break;
            }
        }

        private AlexaResponse NewCoursesIntentHndler(Request request)
        {
            var output = new StringBuilder("Here are the latest courses. ");

            using (var db = new azurealexaEntities())
            {
                db.Courses.Take(10).OrderByDescending(c=>c.DateCreated).ToList().ForEach(c=> output.AppendFormat("{0} by {1}. ", c.Title, c.Author));
            }
            return new AlexaResponse(output.ToString());
        }

        private AlexaResponse TopCoursesIntentHandler(Request request)
        {
            int limit = 10;
            if (request.SlotsList.Any())
            {
                int maxLimit = 10;
                var limitValue = request.SlotsList.FirstOrDefault(s => s.Key == "Limit").Value;

                if (!string.IsNullOrWhiteSpace(limitValue) && int.TryParse(limitValue, out limit) && !(limit >= 1 &&
                    limit <= maxLimit))
                {
                    limit = maxLimit;
                }
            }
            var output = new StringBuilder("");
            output.AppendFormat("Here are the top {0} courses. ", limit);

            using (var db = new azurealexaEntities())
            {
                db.Courses.Take(limit).OrderByDescending(c => c.Votes).ToList().ForEach(c => output.AppendFormat("{0} by {1}. ", c.Title, c.Author));
            }
            return new AlexaResponse(output.ToString());
        }

        private AlexaResponse SessionEndedRequestHandler(Request request)
        {
            return null;
        }

        

       
    }

}
