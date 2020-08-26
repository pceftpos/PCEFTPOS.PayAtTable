using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Common.Logging;
using System.Runtime.CompilerServices;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;
using PayAtTable.Server.Helpers;

namespace PayAtTable.Server.Controllers
{
    [RoutePrefix("api")]
    public class TablesController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly ITablesRepository _tablesRepository;
        protected readonly IClientValidator _clientValidator;

        public TablesController(ITablesRepository tablesRepository, IClientValidator certificateValidator)
        {
            log.DebugEx("TablesController created");
            _tablesRepository = tablesRepository;
            _clientValidator = certificateValidator;
        }

        [HttpGet, Route("tables")]
        public HttpResponseMessage GetTables()
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx("GET ~/api/tables");
            try
            {
                var r = new PATResponse { Tables = _tablesRepository.GetTables() };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in GET ~/api/tables.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set("ResourceNotFoundException in GET ~/api/tables.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
