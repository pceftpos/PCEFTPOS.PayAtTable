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
    public class TendersController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        protected readonly ITendersRepository _tendersRepository;
        protected readonly IClientValidator _clientValidator;

        public TendersController(ITendersRepository tendersRepository, IClientValidator certificateValidator)
        {
            log.DebugEx("TendersController created");
            _tendersRepository = tendersRepository;
            _clientValidator = certificateValidator;
        }

        [HttpPost, Route("tenders")]
        public HttpResponseMessage CreateTender([FromBody]PATRequest tenderRequest)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx(tr => tr.Set("POST ~/api/tenders", tenderRequest));

            // Extract the tender from the request
            if (tenderRequest == null || tenderRequest.Tender == null)
            {
                log.ErrorEx(tr => tr.Set("TenderRequest.Tender==NULL in POST ~/api/tenders."));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "TenderRequest.Tender==NULL");
            }

            try
            {
                var r = new PATResponse { Tender = _tendersRepository.CreateTender(tenderRequest.Tender) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/tenders.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set("ResourceNotFoundException in POST ~/api/tenders.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }

        [HttpPut, Route("tenders/{id}")]
        public HttpResponseMessage UpdateTender(string id, [FromBody]PATRequest tenderRequest)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx(tr => tr.Set("PUT ~/api/tenders", tenderRequest));

            // Extract the tender from the request
            if (tenderRequest == null || tenderRequest.Tender == null)
            {
                log.ErrorEx(tr => tr.Set("TenderRequest.Tender==NULL in PUT ~/api/tenders."));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "TenderRequest.Tender==NULL");
            }

            // Validate the tender id
            if (!tenderRequest.Tender.Id.Equals(id))
            {
                log.ErrorEx(tr => tr.Set("tenderRequest.Tender.Id != param id in PUT ~/api/tenders."));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "tenderRequest.Tender.Id != param id");
            }

            try
            {
                var r = new PATResponse { Tender = _tendersRepository.UpdateTender(tenderRequest.Tender) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set("InvalidRequestException in PUT ~/api/tenders.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set("ResourceNotFoundException in PUT ~/api/tenders.", ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
