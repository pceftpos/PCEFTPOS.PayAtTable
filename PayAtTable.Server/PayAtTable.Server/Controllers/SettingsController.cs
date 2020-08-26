using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Common.Logging;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;
using System.Web;
using PayAtTable.Server.Helpers;

namespace PayAtTable.Server.Controllers
{
    [RoutePrefix("api")]
    public class SettingsController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        protected readonly ISettingsRepository _settingsRepository;
        protected readonly IClientValidator _clientValidator;

        public SettingsController(ISettingsRepository settingsRepository, IClientValidator certificateValidator)
        {
            log.DebugEx("SettingsController created");
            _settingsRepository = settingsRepository;
            _clientValidator = certificateValidator;
        }

        [HttpGet, Route("settings")]
        public HttpResponseMessage GetSettings()
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx("GET ~/api/settings");

            try
            {
                var r = new PATResponse { Settings = _settingsRepository.GetSettings() };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in GET ~/api/settings.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set("ResourceNotFoundException in GET ~/api/settings.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
