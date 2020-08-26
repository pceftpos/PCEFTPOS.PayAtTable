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
    //
    [RoutePrefix("api")]
    public class EFTPOSController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        protected readonly IEFTPOSRepository _eftposRepository;
        protected readonly IClientValidator _clientValidator;

        public EFTPOSController(IEFTPOSRepository eftposRepository, IClientValidator certificateValidator)
        {
            log.DebugEx("EFTPOSController created");
            _eftposRepository = eftposRepository;
            _clientValidator = certificateValidator;
        }

        [HttpPost, Route("eftpos/commands")]
        public HttpResponseMessage CreateEFTPOSCommand([FromBody]PATRequest commandRequest)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx(tr => tr.Set("POST ~/api/eftpos/commands", commandRequest));

            // Extract the eftpos command from the request
            if (commandRequest == null || commandRequest.EFTPOSCommand == null)
            {
                log.ErrorEx(tr => tr.Set("TenderRequest.EFTPOSCommand==NULL in POST ~/api/eftpos/commands."));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "PATRequest.EFTPOSCommand==NULL");
            }

            try
            {
                var r = new PATResponse { EFTPOSCommand = _eftposRepository.CreateEFTPOSCommand(commandRequest.EFTPOSCommand) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/eftpos/commands.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/eftpos/commands.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
