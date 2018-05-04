using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;
using PayAtTable.Server;
using PayAtTable.API.Helpers;
using PayAtTable.Server.Models;

namespace PayAtTable.Server.IntegrationTest
{
    public class Test
    {
        public Test(string baseUri, string apiKey, bool debugTrace, bool delayEFTPOSCommands)
        {
            BaseUri = baseUri;
            ApiKey = apiKey;
            DebugTrace = debugTrace;
            DelayEFTPOSCommands = delayEFTPOSCommands;
        }


        /// <summary>
        /// Get settings, throws an exception if data is invalid
        /// </summary>
        /// <exception cref="Exception"/>
        /// <param name="settings"></param>
        void GetSettings(ref Settings settings)
        {
            // Get settings
            var uri = String.Format("{0}/api/settings?key={1}", BaseUri, ApiKey);
            var r = JsonConvert.DeserializeObject<PATResponse>(GetJsonString(uri, DebugTrace));
            if (r.Settings == null)
                throw new Exception("Settings == null");
            settings = r.Settings;

            // Display receipt options
            if (r.Settings.ReceiptOptions == null || r.Settings.ReceiptOptions.Count == 0)
                Console.WriteLine("No Receipt Options");
            else
            {
                Console.Write("Receipt Options: ");
                foreach (var receiptOption in r.Settings.ReceiptOptions)
                    Console.Write(receiptOption.DisplayName + ",");
                Console.WriteLine("");
            }

            // Display tender options
            if (r.Settings.TenderOptions == null || r.Settings.TenderOptions.Count == 0)
                Console.WriteLine("No Tender Options");
            else
            {
                Console.Write("Tender Options: ");
                foreach (var tenderOption in r.Settings.TenderOptions)
                    Console.Write(tenderOption.DisplayName + ",");
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Gets tables. throws an exception if data is invalid
        /// </summary>
        /// <param name="tables"></param>
        void GetTables(ref List<Table> tables)
        {
            // Get tables
            var uri = String.Format("{0}/api/tables?key={1}", BaseUri, ApiKey);
            var r = JsonConvert.DeserializeObject<PATResponse>(GetJsonString(uri, DebugTrace));
            if (r.Tables == null)
                throw new Exception("Tables == null");
            tables = r.Tables.ToList();
            Console.WriteLine("Tables count {0}", r.Tables.Count());

        }

        /// <summary>
        /// Get first available order from tables, throws an exception if data is invalid
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="order"></param>
        /// <param name="table"></param>
        void GetFirstOrderFromTables(List<Table> tables, ref Order order, ref Table table)
        {
            // Loop through tables until we find an order
            foreach (var t in tables)
            {
                var uri = String.Format("{0}/api/tables/{1}/orders?key={2}", BaseUri, t.Id, ApiKey);
                var r = JsonConvert.DeserializeObject<PATResponse>(GetJsonString(uri, DebugTrace));
                if (r.Orders != null && r.Orders.Count() > 0)
                {
                    order = r.Orders.First();
                    table = t;
                    break;
                }
            }
            // Check if we have an order
            if (order == null)
                throw new Exception("Order == null");
            Console.WriteLine("Order {0} found on Table {1}", order.DisplayName, table.DisplayName);
        }

        /// <summary>
        /// Get default receipt, throws an exception if data is invalid
        /// </summary>
        /// <param name="order"></param>
        /// <param name="receiptOption"></param>
        void GetReceipt(Order order, ReceiptOption receiptOption)
        {
            var uri = String.Format("{0}/api/orders/{1}/receipt?key={2}", BaseUri, order.Id, ApiKey);
            var r = JsonConvert.DeserializeObject<PATResponse>(GetJsonString(uri, DebugTrace));
            if (r.Receipt == null || r.Receipt.Lines == null || r.Receipt.Lines.Count == 0)
                throw new Exception("No receipt");
            foreach (var l in r.Receipt.Lines)
                Console.WriteLine(l);
        }

        /// <summary>
        /// Create a tender, throws an exception if data is invalid
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="tenderOption"></param>
        void CreateTender(ref Tender tender, TenderOption tenderOption, Order order)
        { 
            tender = new Tender() { AmountPurchase = order.AmountOwing, OriginalAmountPurchase = order.AmountOwing, TenderState = TenderState.Pending, OrderId = order.Id };
            var uri = String.Format("{0}/api/tenders?key={1}", BaseUri, ApiKey);
            var r = JsonConvert.DeserializeObject<PATResponse>(PostJsonString(uri, JsonConvert.SerializeObject(new PATRequest() { Tender = tender }), DebugTrace));
            if (r.Tender == null)
                throw new Exception("No tender in POST response");
            // Update our tender with the details from the response
            tender = r.Tender;
            Console.WriteLine("Tender created");
        }

        /// <summary>
        /// Create a "DoTransaction" EFTPOS command, throws an exception if data is invalid
        /// </summary>
        /// <param name="eftposRequestCommand"></param>
        /// <param name="tender"></param>
        void CreateEFTPOSRequest(ref EFTPOSCommand eftposRequestCommand, Tender tender)
        {
            var uri = String.Format("{0}/api/eftpos/commands?key={1}", BaseUri, ApiKey);
            eftposRequestCommand = new EFTPOSCommand() { EFTPOSCommandType = EFTPOSCommandType.DoTransaction, TenderId = tender.Id, AmtPurchase = tender.AmountPurchase, TxnType = "P", TxnRef = String.Format("{0:yyddMMHHmmsszzz}", DateTime.Now) };
            var r = JsonConvert.DeserializeObject<PATResponse>(PostJsonString(uri, JsonConvert.SerializeObject(new PATRequest() { EFTPOSCommand = eftposRequestCommand }), DebugTrace));
            if (r.EFTPOSCommand == null)
                throw new Exception("No EFTPOSCommand in POST response");
            Console.WriteLine("DoTransaction EFTPOSCommand Created");
        }

        /// <summary>
        /// Create a "TransactionEvent" EFTPOS command, throws an exception if data is invalid
        /// </summary>
        /// <param name="eftposEventCommand"></param>
        /// <param name="eftposRequestCommand"></param>
        /// <param name="tender"></param>
        void CreateEFTPOSEvent(ref EFTPOSCommand eftposEventCommand, EFTPOSCommand eftposRequestCommand, Tender tender)
        {
            var uri = String.Format("{0}/api/eftpos/commands?key={1}", BaseUri, ApiKey);
            eftposEventCommand = new EFTPOSCommand()
            {
                EFTPOSCommandType = EFTPOSCommandType.TransactionEvent,
                TenderId = tender.Id,
                OriginalEFTPOSCommandId = eftposRequestCommand.Id,
                AmtPurchase = eftposRequestCommand.AmtPurchase,
                TxnType = "P",
                STAN = "26",
                ResponseType = "210",
                Caid = "000008156005003",
                Catid = "00000000",
                Date = "000000",
                Time = "000000",
                ResponseCode = "00",
                ResponseText = "APPROVED",
                Merchant = "0",
                Success = true,
                AccountType = "Credit",
                CardType = "DEBIT CARD AC",
                CardName = "00",
                TxnRef = eftposRequestCommand.TxnRef,
                DateSettlement = "000000",
                AuthCode = "000023",
                CsdReservedBool1 = false,
                Rrn = "000000000023",
                Track2 = "4343000000000005=0101?",
                Pan = "4343000000000005"
            };
            var r = JsonConvert.DeserializeObject<PATResponse>(PostJsonString(uri, JsonConvert.SerializeObject(new PATRequest() { EFTPOSCommand = eftposEventCommand }), DebugTrace));
            if (r.EFTPOSCommand == null)
                throw new Exception("No EFTPOSCommand in POST response");
            Console.WriteLine("TransactionEvent EFTPOSCommand Created");
        }

        /// <summary>
        /// Updates tender as complete. Throws an exception if data is invalid
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="order"></param>
        void UpdateTender(ref Tender tender, Order order)
        {
            var uri = String.Format("{0}/api/tenders/{1}?key={2}", BaseUri, tender.Id, ApiKey);
            tender.TenderState = TenderState.CompleteSuccessful;
            var r = JsonConvert.DeserializeObject<PATResponse>(PutJsonString(uri, JsonConvert.SerializeObject(new PATRequest() { Tender = tender }), DebugTrace));
            if (r.Tender == null)
                throw new Exception("No tender in PUT response");
            // Update our tender with the details from the response
            tender = r.Tender;
            Console.WriteLine("Tender updated");
        }

        /// <summary>
        /// Validates that the given order is complete. Throws an exception if it's not
        /// </summary>
        /// <param name="order"></param>
        void ValidateOrderComplete(Order order)
        {
            var uri = String.Format("{0}/api/orders/{1}?key={2}", BaseUri, order.Id, ApiKey);
            var r = JsonConvert.DeserializeObject<PATResponse>(GetJsonString(uri, DebugTrace));
            if (r.Order == null)
                throw new Exception("No order");
            if (r.Order.AmountOwing != 0)
                throw new Exception("Order amount <> 0");
            if (r.Order.OrderState != OrderState.Complete)
                throw new Exception("Order state <> complete");
        }

        public void Run()
        {
            //PATResponse r = null;
            List<Table> tables = null;
            Order order = null;
            Table table = null;
            Tender tender = null;
            Settings settings = null;
            string uri = null;
            EFTPOSCommand eftposRequestCommand = null, eftposEventCommand = null;
            string eftposCommandId = string.Empty;

            // Get our settings
            GetSettings(ref settings);

            // Get tables
            GetTables(ref tables);

            // Find the first available order
            GetFirstOrderFromTables(tables, ref order, ref table);

            // Grab a receipt for this order
            if (settings.ReceiptOptions != null && settings.ReceiptOptions.Count > 0)
            {
                GetReceipt(order, settings.ReceiptOptions[0]);
            }

            // Create a tender (we can't continue without a default tender option)
            if (settings.TenderOptions == null || settings.TenderOptions.Count == 0)
            {
                throw new Exception("Can't continue without a tender option");
            }
            CreateTender(ref tender, settings.TenderOptions[0], order);

            // Send our txn request EFTPOS command\
            CreateEFTPOSRequest(ref eftposRequestCommand, tender);
            eftposCommandId = eftposRequestCommand.Id; 
             
            // Send some display events
            uri = String.Format("{0}/api/eftpos/commands?key={1}", BaseUri, ApiKey);
            PostDisplayEvent(uri, DelayEFTPOSCommands, DebugTrace, tender.Id, eftposRequestCommand.Id, "     SWIPE CARD                         ", "100000003", "0");
            PostDisplayEvent(uri, DelayEFTPOSCommands, DebugTrace, tender.Id, eftposRequestCommand.Id, "   SELECT ACCOUNT                       ", "100000004", "0");
            PostDisplayEvent(uri, DelayEFTPOSCommands, DebugTrace, tender.Id, eftposRequestCommand.Id, "     ENTER  PIN                         ", "100000005", "0");
            PostDisplayEvent(uri, DelayEFTPOSCommands, DebugTrace, tender.Id, eftposRequestCommand.Id, "     PROCESSING         PLEASE WAIT     ", "000000000", "0");
            PostDisplayEvent(uri, DelayEFTPOSCommands, DebugTrace, tender.Id, eftposRequestCommand.Id, "     TRANSACTION          APPROVED      ", "100000005", "0");
            
            // Send the transaction event
            CreateEFTPOSEvent(ref eftposEventCommand, eftposRequestCommand, tender);

            // Update the tender
            UpdateTender(ref tender, order);

            // Check the order details. It should be complete. 
            ValidateOrderComplete(order);

            Console.WriteLine("Complete");
        }

        protected void PostDisplayEvent(string uri, bool delayEFTPOSCommands, bool debugTrace, string tenderId, string originalEFTPOSCommandId, string dataField, string csdReservedString1, string csdReservedString5)
        {
            var eftposEventCommand = new EFTPOSCommand() 
            { 
                EFTPOSCommandType = EFTPOSCommandType.DisplayEvent, 
                TenderId = tenderId,
                OriginalEFTPOSCommandId = originalEFTPOSCommandId, 
                DataField = dataField,
                CsdReservedString1 = csdReservedString1,
                CsdReservedString5 = csdReservedString5 
            };

            var r = JsonConvert.DeserializeObject<PATResponse>(PostJsonString(uri, JsonConvert.SerializeObject(new PATRequest() { EFTPOSCommand = eftposEventCommand }), debugTrace));
            if (r.EFTPOSCommand == null)
                throw new Exception("No EFTPOSCommand in POST response");
            if (delayEFTPOSCommands) System.Threading.Thread.Sleep(1000);

        }

        protected string PostJsonString(string uri, string content, bool debugTrace)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(content, Encoding.UTF8, "text/json");
                
                if(debugTrace)
                {
                    Console.WriteLine("POST {0}", uri);
                    Console.WriteLine(content);
                    Console.WriteLine();
                }
                
                var result = client.SendAsync(request).Result;
                result.EnsureSuccessStatusCode();
                var resultContent = result.Content.ReadAsStringAsync().Result;

                if (debugTrace)
                {
                    Console.WriteLine("RESPONSE {0}", result.StatusCode);
                    Console.WriteLine(resultContent);
                    Console.WriteLine();
                }

                return resultContent;
            }
        }

        protected string PutJsonString(string uri, string content, bool debugTrace)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Put, uri);
                request.Content = new StringContent(content, Encoding.UTF8, "text/json");

                if (debugTrace)
                {
                    Console.WriteLine("PUT {0}", uri);
                    Console.WriteLine(content);
                    Console.WriteLine();
                }

                var result = client.SendAsync(request).Result;
                result.EnsureSuccessStatusCode();
                var resultContent = result.Content.ReadAsStringAsync().Result;

                if (debugTrace)
                {
                    Console.WriteLine("RESPONSE {0}", result.StatusCode);
                    Console.WriteLine(resultContent);
                    Console.WriteLine();
                }

                return resultContent;
            }
        }

        protected string GetJsonString(string uri, bool debugTrace)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);

                if (debugTrace)
                {
                    Console.WriteLine("GET {0}", uri);
                }                
                
                var result = client.SendAsync(request).Result;
                result.EnsureSuccessStatusCode();
                var resultContent = result.Content.ReadAsStringAsync().Result;

                if (debugTrace)
                {
                    Console.WriteLine("RESPONSE {0}", result.StatusCode);
                    Console.WriteLine(resultContent);
                    Console.WriteLine();
                }

                return resultContent;
            }
        }


        public string ApiKey { get; set; }
        public string BaseUri { get; set; }
        public bool DebugTrace { get; set; }
        public bool DelayEFTPOSCommands { get; set; }
    }



    class Program
    {
        static void Main(string[] args)
        {
            string baseUri = null, apiKey = null;
            bool debugTrace = false, delayEFTPOSCommands = false, showHelp = false;

            var p = new OptionSet() 
            {
            { "baseUri=", "the {BASEURI} to connect to.", v => baseUri = v },
            { "key=", "the {APIKEY} to use.", v => apiKey = v },
            { "debugTrace", "enable debug tracing", v => debugTrace = (v != null) },
            { "delayEFTPOSCommands", "insert a delay between each EFTPOS display", v => delayEFTPOSCommands = (v != null) },
            { "help",  "show this message and exit", v => showHelp = (v != null) }
            };

            try
            { 
                p.Parse(args);
            }
            catch(OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try '--help' for options.");
                return;
            }

            if(showHelp)
            {
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (baseUri == null)
            {
                Console.WriteLine("baseUri parameter must be set");
                Console.WriteLine("Try '--help' for options.");
                return;
            }

            try
            {
                new Test(baseUri, apiKey, debugTrace, delayEFTPOSCommands).Run();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
