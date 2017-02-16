using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using PayAtTable.Server.Models;

namespace PayAtTable.TestPos.IPInterface
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestType { GET, PUT, POST, DELETE };
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestMethod { Settings, Tables, TablesWithOrders, TableOrders, Order, OrderReceipt, Tender, EFTPOSCommand };
    public enum ResponseCode { Ok = 200, Accepted = 201, NoContent = 204, BadRequest = 400, Unauthorized = 401, Forbidden = 403, NotFound = 404, ServerError = 500 };

    public class POSAPIMsgHeader
    {
        /// Pay @ Table message version. Default to 1
        public int Version { get; set; } = 1;

        /// Defines the data format of the "content" field. Default application/json
        public string ContentType { get; set; } = "application/json";

        // Defines the type of request. GET, PUT, POST, DELETE
        public RequestType RequestType { get; set; } = RequestType.GET;

        // Defines the request method. Settings, Tables, TableOrders, Order, OrderReceipt, Tender, EFTPOSCommand
        public RequestMethod RequestMethod { get; set; } = RequestMethod.Settings;

        // The length of the content to follow
        public int ContentLength { get; set; } = 0;

        public string TableId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ReceiptOptionId { get; set; } = string.Empty;

        public Tender tender { get; set; } = new Tender();

        // Only returned in a response header. Defines the message success. One of the HTTP response codes. 200, 201, 204, 400, 401, 403, 404, 500.
        ResponseCode responseCode = ResponseCode.Ok;
        public ResponseCode ResponseCode
        {
            get
            {
                return responseCode;
            }
            set
            {
                responseCode = value;
                ResponseText = Enum.GetName(typeof(ResponseCode), value);
            }
        }

        // One of the HTTP response codes texts
        public string ResponseText { get; set; } = "OK";

        public override string ToString()
        {
            var header = JsonConvert.SerializeObject(this);
            return header.Length.ToString("000000") + header;
        }
    }

    public class POSAPIMsg
    {
        public POSAPIMsgHeader Header { get; set; } = new POSAPIMsgHeader();
        public string Content { get; set; } = "";

        public override string ToString()
        {
            Header.ContentLength = Content?.Length ?? 0;
            var header = JsonConvert.SerializeObject(Header);
            return header.Length.ToString("000000") + header + Content;
        }


        public bool ParseFromString(string v)
        {
            int headerLength = 0;
            // Validate total length
            if (v.Length < 6)
                return false;
            // Unpack header length
            int.TryParse(v.Substring(0, 6), out headerLength);
            // Validate header length
            if (v.Length < (6 + headerLength))
                return false;
            // Unpack header
            Header = Newtonsoft.Json.JsonConvert.DeserializeObject<POSAPIMsgHeader>(v.Substring(6, headerLength));
            // Validate content length

            if (v.Length < (6 + headerLength + Header.ContentLength))
            {
               // Log($"RX: {eft.DataField}");
                return false;
            }
            // Unpack content
            Content = v.Substring(6 + headerLength, Header.ContentLength);

            return true;
        }

        public bool ParseFromString(PCEFTPOS.API.IPInterface.EFTPayAtTableResponse response)
        {
            Header = JsonConvert.DeserializeObject<POSAPIMsgHeader>(response.Header);
            Content = response.Content;

            return true;
        }
    }
}