using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
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

                var response = new AlexaResponse("Hello world! Would you like for me to say it again? Yes or no?")
                {
                    Session = {MemberId = request.MemberId}
                };
                response.Response.Card.Title = "Pluralsight";
                response.Response.Card.Content = "Hello\ncruel world!";
                response.Response.Reprompt.OutputSpeech.Text = "Say it again? Yes or no?";
                response.Response.ShouldEndSession = false;

                if (request.Intent == "AMAZON.NoIntent")
                {
                    response.Response.OutputSpeech.Text = "Ok, have a nice day.";
                    response.Response.ShouldEndSession = true;
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
    }

}
