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


namespace PayAtTable.Server.Controllers
{
    [RoutePrefix("api")]
    public class SettingsController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        protected readonly ISettingsRepository _settingsRepository;

        public SettingsController(ISettingsRepository settingsRepository)
        {
            log.DebugEx("SettingsController created");
            _settingsRepository = settingsRepository;
        }

        [HttpGet, Route("settings")]
        public HttpResponseMessage GetSettings()
        {
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
