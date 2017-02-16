using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSDEFTLib;
using PayAtTable.Server.Data;
using PayAtTable.Server.DemoRepository;
using PayAtTable.Server.Models;

using Newtonsoft.Json;
using System.IO;
using PayAtTable.TestPos.ViewModels;

namespace PayAtTable.TestPos
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Data access objects (this is just pulled directly from the PayAtTable.Server sample code)
        ITablesRepository tablesRepository = new TablesRepositoryDemo();
        IOrdersRepository ordersRepository = new OrdersRepositoryDemo();
        ITendersRepository tendersRepository = new TendersRepositoryDemo();
        IEFTPOSRepository eftposRepository = new EFTPOSRepositoryDemo();
        ISettingsRepository settingsRepository = new SettingsRepositoryDemo();

        //some debug ...
        //long TenderIDCreate;
        //long TenderIDUpdate;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("Start up");
            eft.DoStatus();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void eft_StatusEvent(object sender, EventArgs e)
        {
            Log($"Status response. {eft.ResponseCode} {eft.ResponseText}", (eft.Success ? ViewModels.LogType.INFO : ViewModels.LogType.ERROR));
            Log($"Pinpad online: {eft.Success}");
        }

        private void eft_CsdReserved3(object sender, EventArgs e)
        {
            if (eft.TxnType != "@")
                return;

            //cpk
            // The Pay @ Table extension is asking us for some data

            Log(new LogData(eft.DataField, $"RX (Received): {eft.DataField}"));

            // Unpack request
            var requ = new POSAPIMsg();
            requ.ParseFromString(eft.DataField);

            // Handle request
            switch (requ.Header.RequestMethod)
            {
                case RequestMethod.Settings:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetSettings(requ);
                    break;
                case RequestMethod.Tables:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetTables(requ);
                    break;
                case RequestMethod.TablesWithOrders:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetTablesWithOrders(requ);
                    break;

                case RequestMethod.TableOrders:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetTableOrders(requ);
                    break;


                case RequestMethod.Order:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetOrder(requ);
                    break;

                case RequestMethod.OrderReceipt:
                    if (requ.Header.RequestType == RequestType.GET)
                        HandleGetOrderReceipt(requ);
                    break;

                case RequestMethod.Tender:
                    if (requ.Header.RequestType == RequestType.POST)
                    {
                        HandleCreateTender(requ);
                    }
                    else if (requ.Header.RequestType == RequestType.PUT)
                        HandleUpdateTender(requ);
                    break;

                //cpk to do yeah probably need do this prior Tender update 

                case RequestMethod.EFTPOSCommand:
                    if (requ.Header.RequestType == RequestType.POST) //looks like there are 3 of them to handle 
                        HandleEFTPOSCommand(requ);
                    break;


            }
        }

        void HandleRequest(POSAPIMsg request, Func<PATResponse> func)
        {
            Log($"Processing {request.Header.RequestType} {request.Header.RequestMethod}");

            // Get content
            ResponseCode rc = ResponseCode.Ok;
            string content = null;

            try
            {
                PATResponse patResponse = func.Invoke();
                content = JsonConvert.SerializeObject(patResponse);
            }
            catch (InvalidRequestException ex)
            {
                Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.BadRequest;
            }
            catch (ResourceNotFoundException ex)
            {
                Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.NotFound;
            }
            catch (Exception ex)
            {
                Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.ServerError;
            }

            // Build response
            POSAPIMsg resp = new POSAPIMsg()
            {
                Header = new POSAPIMsgHeader()
                {
                    RequestMethod = request.Header.RequestMethod,
                    RequestType = request.Header.RequestType,
                    ResponseCode = content != null ? ResponseCode.Ok : rc,
                    ContentLength = content?.Length ?? 0
                },
                Content = content
            };

            var respString = resp.ToString();

            Log(new LogData(respString, $"TX (Response): {respString}"));

            eft.TxnType = "@";
            eft.CsdReservedString1 = respString;
            eft.CsdReservedMethod3();
        }

        private void HandleEFTPOSCommand(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);
            //if (patRequest == null || patRequest.Tender == null)
            //{
            //    Log($"Invalid Request found in: {request.Header.RequestType} {request.Header.RequestMethod}", LogType.ERROR);
            //    return;
            //}

            HandleRequest(request, () => new PATResponse { EFTPOSCommand = eftposRepository.CreateEFTPOSCommand(patRequest.EFTPOSCommand) });

            //Log($"POST EFTPOS COMMAND");
            //bool Error = false;

            //ResponseCode rc = ResponseCode.Ok;
            //PATResponse patResponse = new PATResponse();
            //string content = null;
            //// Create tender
            //try
            //{
            //    // Extract the tender from the request
            //    var patRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<PATRequest>(requ.Content);

            //    // Validate tender obect in request
            //    if (patRequest == null || patRequest.EFTPOSCommand == null)
            //    {
            //        Log("TenderRequest.Tender==NULL in POST ~/api/tenders.");
            //        rc = ResponseCode.BadRequest;
            //    }

            //    // Build content response
            //    patResponse = new PATResponse { EFTPOSCommand = eftposRepository.CreateEFTPOSCommand(patRequest.EFTPOSCommand) };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //    // tendersRepository.UpdateTender(UpdatedTender);

            //}
            //catch (InvalidRequestException ex)
            //{
            //    //log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.BadRequest;
            //    Error = true;
            //}
            //catch (ResourceNotFoundException ex)
            //{
            //    //log.ErrorEx(tr => tr.Set("ResourceNotFoundException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.NotFound;
            //    Error = true;
            //}
            //catch (Exception e)
            //{
            //    Log($">>>>>>>>>>>>>>>>BUGGER Processing EFTPOS COMMAND exception {e.Message}");
            //    Log($"Dumping requ.Content {requ.Content} ");

            //    rc = ResponseCode.ServerError;
            //    Error = true;
            //}

            //if (Error == false)
            //{
            //    Log($" Sending back EFTPOS COMMAND RESPONSE");
            //    Log($">>OK<< Dumping requ.Content {requ.Content} ");
            //    SendResponse(requ, content);

            //}
            //else
            //{
            //    Log($">>ERROR<< EFTPOS COMMAND RESPONSE");
            //    SendResponse(requ, content);
            //}

        }


        private void HandleUpdateTender(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);
            if (patRequest == null || patRequest.Tender == null)
            {
                Log($"Invalid Request found in: {request.Header.RequestType} {request.Header.RequestMethod}", LogType.ERROR);
                return;
            }

            HandleRequest(request, () => new PATResponse { Tender = tendersRepository.UpdateTender(patRequest.Tender) });

            //Log($"UPDATE TENDER");
            //Log($"PUT Tender");

            //ResponseCode rc = ResponseCode.Ok;
            //PATResponse patResponse = new PATResponse();

            //Tender UpdatedTender = new Tender();
            //// Create tender
            //try
            //{
            //    // Extract the tender from the request
            //    var patRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<PATRequest>(requ.Content);

            //    // Validate tender obect in request
            //    if (patRequest == null || patRequest.Tender == null)
            //    {
            //        Log("TenderUpdate.Tender==NULL in POST ~/api/tenders.");
            //        rc = ResponseCode.BadRequest;
            //    }

            //    // Build content response
            //    // patResponse = new PATResponse { Tender = tendersRepository.CreateTender(patRequest.Tender) };
            //    patResponse = new PATResponse { Tender = tendersRepository.UpdateTender(patRequest.Tender) };
            //    //save for later processing
            //    UpdatedTender = patRequest.Tender;
            //    TenderIDUpdate = patRequest.Tender.Id;

            //}
            //catch (InvalidRequestException ex)
            //{
            //    Log($"CPK ERROR 1");
            //    //log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.BadRequest;
            //}
            //catch (ResourceNotFoundException ex)
            //{
            //    Log($"CPK ERROR 2");

            //    //log.ErrorEx(tr => tr.Set("ResourceNotFoundException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.NotFound;
            //}
            //catch (Exception e)
            //{
            //    Log($"CPK ERROR 3");

            //    Log($"Exception. {e.Message}");
            //    rc = ResponseCode.ServerError;
            //}


            //Log($"CPK todo prior to sending response update info from tender (deduct amts owing)");

            //if (UpdatedTender.TenderState == TenderState.CompleteSuccessful)//update values 
            //{
            //    if (TenderIDCreate != TenderIDUpdate)
            //    {
            //        Log($"CPK ERROR TENDER ID MIS MATCH SENT VS UPDATE ONE RX {UpdatedTender.Id} {UpdatedTender.Id}");
            //    }
            //    else
            //    {
            //        Log($"CPK updating values)");
            //        Log($"Tender ID and Order ID. {UpdatedTender.Id}  {UpdatedTender.OrderId}");
            //        Log($"CPK updating values)");
            //        tendersRepository.UpdateTender(UpdatedTender);//success updated received tender
            //    }

            //}
            //else
            //    Log($"Up Dated Tender State  {UpdatedTender.TenderState}");


            ////CPK todo prior to sending response update info from tender (deduct amts owing )

            //SendPATResponse(requ, patResponse, rc);


        }

        private void HandleCreateTender(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);
            if (patRequest == null || patRequest.Tender == null)
            {
                Log($"Invalid Request found in: {request.Header.RequestType} {request.Header.RequestMethod}", LogType.ERROR);
                return;
            }

            HandleRequest(request, () => new PATResponse { Tender = tendersRepository.CreateTender(patRequest.Tender) });

            //Log($"POST Tender");

            //ResponseCode rc = ResponseCode.Ok;
            //PATResponse patResponse = new PATResponse();

            //// Create tender
            //try
            //{
            //    // Extract the tender from the request
            //    var patRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<PATRequest>(requ.Content);

            //    // Validate tender obect in request
            //    if (patRequest == null || patRequest.Tender == null)
            //    {
            //        Log("TenderRequest.Tender==NULL in POST ~/api/tenders.");
            //        rc = ResponseCode.BadRequest;
            //    }

            //    // Build content response
            //    patResponse = new PATResponse { Tender = tendersRepository.CreateTender(patRequest.Tender) };
            //    // tendersRepository.UpdateTender(UpdatedTender);
            //    //cpk debug remove
            //    TenderIDCreate = patRequest.Tender.Id;

            //}
            //catch (InvalidRequestException ex)
            //{
            //    //log.ErrorEx(tr => tr.Set("InvalidRequestException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.BadRequest;
            //}
            //catch (ResourceNotFoundException ex)
            //{
            //    //log.ErrorEx(tr => tr.Set("ResourceNotFoundException in POST ~/api/tenders.", ex));
            //    rc = ResponseCode.NotFound;
            //}
            //catch (Exception e)
            //{
            //    Log($"Exception. {e.Message}");
            //    rc = ResponseCode.ServerError;
            //}

            //SendPATResponse(requ, patResponse, rc);


        }

        //private void SendPATResponse(POSAPIMsg requ, PATResponse patResponse, ResponseCode rc)
        //{
        //    // Build response
        //    var content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
        //    POSAPIMsg resp = new POSAPIMsg()
        //    {
        //        Header = new POSAPIMsgHeader()
        //        {
        //            RequestMethod = requ.Header.RequestMethod,
        //            RequestType = requ.Header.RequestType,
        //            ResponseCode = rc,
        //            ContentLength = content?.Length ?? 0
        //        },
        //        Content = content
        //    };

        //    var respString = resp.ToString();
        //    //Log($"TX (Response): {respString}");
        //    Log(new LogData(eft.DataField, $"TX (Response): {resp.Content}"));

        //    eft.TxnType = "@";
        //    eft.CsdReservedString1 = respString;
        //    eft.CsdReservedMethod3();

        //    //            return;
        //}

        //private void SendResponse(POSAPIMsg requ, string content)
        //{

        //    // Build response
        //    POSAPIMsg resp = new POSAPIMsg()
        //    {
        //        Header = new POSAPIMsgHeader()
        //        {
        //            RequestMethod = requ.Header.RequestMethod,
        //            RequestType = requ.Header.RequestType,
        //            ResponseCode = content != null ? ResponseCode.Ok : ResponseCode.ServerError,
        //            ContentLength = content?.Length ?? 0
        //        },
        //        Content = content
        //    };


        //    var respString = resp.ToString();
        //    //Log($"TX (Response): {respString}");
        //    Log(new LogData(respString, $"TX (Response): {respString}"));

        //    eft.TxnType = "@";
        //    eft.CsdReservedString1 = respString;
        //    eft.CsdReservedMethod3();
        //}

        

        // cpk to retrieve table id and get order(s) it need the table id sent from TPP device 

        private void HandleGetTableOrders(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Orders = ordersRepository.GetOrdersFromTable(request.Header.TableId) });
            //Log($"GET TableOrders");
            //Log($"---: {"GETTING TABLE ORDER (S)"}");
            ////Log($"requ info>>: {requ.ToString()}");
            //Log(new LogData(requ.ToString(), $"requ info>>: {requ.ToString()}"));

            //Log($"Table ID >>: {requ.Header.TableId.ToString()}");
            //Log($"Order ID >>: {requ.Header.OrderId.ToString()}");
            //// Get content
            //string content = null;
            ////cpk TODO need to extrapolate the order id
            ////long TableId = requ.Header.TableId; //cpk fix me 
            
            //try
            //{
            //    var patResponse = new PATResponse() { Orders = ordersRepository.GetOrdersFromTable(requ.Header.TableId) };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //}


            //catch (Exception ex)
            //{
            //    //Log($"Exception. {e.Message}");
            //    Log(ex.Message, LogType.ERROR);
            //    content = null;
            //}

            //Log(new LogData(content, $"TableInfo Content >>: {content}"));


            //SendResponse(requ, content);

           
        }

        //receipt info to print
        private void HandleGetOrderReceipt(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Receipt = ordersRepository.GetCustomerReceiptFromOrderId(request.Header.OrderId, request.Header.ReceiptOptionId) });

            //Log($"HandleGetOrderReceipt");
            //Log($"---: {"PRINT RECEIPT "}");
            ////Log($"requ info>>: {requ.ToString()}");
            //Log(new LogData(requ.ToString(), $"requ info>>: {requ.ToString()}"));

            //Log($"Table ID >>: {requ.Header.TableId.ToString()}");
            //Log($"Order ID >>: {requ.Header.OrderId.ToString()}");
            //string content = null;

            //long OrderId = requ.Header.OrderId; //cpk fix me 
            //try
            //{
            //    var patResponse = new PATResponse() { Receipt = ordersRepository.GetCustomerReceiptFromOrderId(OrderId, 0) };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //}
               
          
            //catch (Exception e)
            //{
            //    Log($"Exception. {e.Message}");
            //    content = null;
            //}


            ////  cpk to do receipt stuff
            //SendResponse(requ, content);

        }


        private void HandleGetOrder(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Order = ordersRepository.GetOrder(request.Header.OrderId) });
            //Log($"GET Order");
            //Log($"---: {"GETTING  ORDER "}");
            ////Log($"requ info>>: {requ.ToString()}");
            //Log(new LogData(requ.ToString(), $"requ info>>: {requ.ToString()}"));
            //// Get content
            //string content = null;

            //ResponseCode rc = ResponseCode.Ok;
            //long OrderId = requ.Header.OrderId; //cpk fix me 
            //PATResponse patResponse = new PATResponse();
            //try
            //{
            //    patResponse = new PATResponse() { Order = ordersRepository.GetOrder(OrderId) };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //    Log($"OrderID AND TABLE ID Found>>: {OrderId.ToString()}  {requ.Header.TableId.ToString()}");
            //}
            //catch (Exception e)
            //{
            //     rc = ResponseCode.NotFound;
            //    Log($"Exception. {e.Message}");
            //    Log($"FAILED TO FING ORDER: {OrderId.ToString()}  {requ.Header.TableId.ToString()}");
            //    content = null;
            //}

            //SendPATResponse(requ, patResponse, rc);

        }

        private void HandleGetTables(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Tables = tablesRepository.GetTables() });
            //Log($"GET Tables");

            //// Get content
            //string content = null;

            ////cpk TODO
            //try
            //{
            //    var patResponse = new PATResponse() { Tables = tablesRepository.GetTables() };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //}
            //catch (Exception e)
            //{
            //    Log($"Exception. {e.Message}");
            //    content = null;
            //}

            //// Build response
            //POSAPIMsg resp = new POSAPIMsg()
            //{
            //    Header = new POSAPIMsgHeader()
            //    {
            //        RequestMethod = requ.Header.RequestMethod,
            //        RequestType = requ.Header.RequestType,
            //        ResponseCode = content != null ? ResponseCode.Ok : ResponseCode.ServerError,
            //        ContentLength = content?.Length ?? 0
            //    },
            //    Content = content
            //};


            //var respString = resp.ToString();
            ////Log($"TX (Response): {respString}");
            //Log(new LogData(resp, $"TX (Response): {respString}"));

            //eft.TxnType = "@";
            //eft.CsdReservedString1 = respString;
            //eft.CsdReservedMethod3();
        }

        private void HandleGetSettings(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Settings = settingsRepository.GetSettings(myApiDataViewModel.Options) });
        }

        private void HandleGetTablesWithOrders(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Tables = tablesRepository.GetTablesWithOrders() });

            //Log($"GET Tables with Orders");

            //// Get content
            //string content = null;
            //try
            //{
            //    var patResponse = new PATResponse() { Tables = tablesRepository.GetTablesWithOrders() };
            //    content = Newtonsoft.Json.JsonConvert.SerializeObject(patResponse);
            //}
            //catch (Exception e)
            //{
            //    Log($"Exception. {e.Message}");
            //    content = null;
            //}

            //// Build response
            //POSAPIMsg resp = new POSAPIMsg()
            //{
            //    Header = new POSAPIMsgHeader()
            //    {
            //        RequestMethod = r.Header.RequestMethod,
            //        RequestType = r.Header.RequestType,
            //        ResponseCode = content != null ? ResponseCode.Ok : ResponseCode.ServerError,
            //        ContentLength = content?.Length ?? 0
            //    },
            //    Content = content
            //};

            //var respString = resp.ToString();
            ////Log($"TX (Response): {respString}");
            //Log(new LogData(respString, $"TX (Response): {respString}"));

            //eft.TxnType = "@";
            //eft.CsdReservedString1 = respString;
            //eft.CsdReservedMethod3();
        }

        void Log(string s, ViewModels.LogType type = ViewModels.LogType.INFO)
        {
            //txtLog.Text += DateTime.Now.ToString("hh:mm:ss tt") + "->" + s + Environment.NewLine;
            //txtLog.Focus();
            //txtLog.SelectionStart = txtLog.Text.Length;
            Log(new LogData(s, s, type));
        }

        void Log(LogData data)
        {
            myApiDataViewModel.Log(data);
            lvLog.SelectedIndex = lvLog.Items.Count;
            lvLog.ScrollIntoView(lvLog.SelectedItem);
        }

        private void btnSendSettings_Click(object sender, RoutedEventArgs e)
        {
            var resp = new POSAPIMsg() { Header = new POSAPIMsgHeader() { RequestMethod = RequestMethod.Settings, RequestType = RequestType.GET } };
            HandleGetSettings(resp);
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            eft.DoStatus();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            myApiDataViewModel.Logs.Clear();
        }
    }
}
