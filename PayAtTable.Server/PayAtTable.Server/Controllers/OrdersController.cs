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
    public class OrdersController : ApiController
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
        protected readonly IOrdersRepository _ordersRepository;
        protected readonly IClientValidator _clientValidator;

        public OrdersController(IOrdersRepository ordersRepository, IClientValidator certificateValidator)
        {
            log.DebugEx("OrdersController created");
            _ordersRepository = ordersRepository;
            _clientValidator = certificateValidator;
        }

        [HttpGet, Route("tables/{id}/orders")]
        public HttpResponseMessage GetOrdersByTableId(string id)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx(tr => tr.Set(String.Format("GET ~/api/tables/{0}/orders", id)));
            try
            {
                var r = new PATResponse { Orders = _ordersRepository.GetOrdersFromTable(id) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("InvalidRequestException in GET ~/api/tables/{0}/orders", id), ex));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("ResourceNotFoundException in GET ~/api/tables/{0}/orders", id), ex));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }

        [HttpGet, Route("orders/{id}")]
        public HttpResponseMessage GetOrder(string id)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            log.DebugEx(tr => tr.Set(String.Format("GET ~/api/orders/{0}", id)));
            try
            {
                var r = new PATResponse() { Order = _ordersRepository.GetOrder(id) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("InvalidRequestException in GET ~/api/orders/{0}", id)));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("ResourceNotFoundException in GET ~/api/orders/{0}", id)));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }

        [HttpGet, Route("orders/{id}/receipt")]
        public HttpResponseMessage GetOrderCustomerReceipt(string id, string receiptOptionId = null)
        {
            if (!_clientValidator.Validate(Request.GetClientCertificate(), Request.Headers.Authorization, out string message))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException(message));

            var option = (!string.IsNullOrEmpty(receiptOptionId) ? receiptOptionId : string.Empty);
            log.DebugEx(tr => tr.Set(String.Format("GET ~/api/orders/{0}/receipt?receiptOptionId={1}", id, option)));

            try
            {
                var r = new PATResponse() { Receipt = _ordersRepository.GetCustomerReceiptFromOrderId(id, receiptOptionId ?? string.Empty) };
                log.DebugEx(tr => tr.Set("return", r));
                return Request.CreateResponse(HttpStatusCode.OK, r);
            }
            catch (InvalidRequestException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("InvalidRequestException in GET ~/api/orders/{0}/receipt?receiptOptionId={1}", id, option)));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                log.ErrorEx(tr => tr.Set(String.Format("ResourceNotFoundException in GET ~/api/orders/{0}/receipt?receiptOptionId={1}", id, option)));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
