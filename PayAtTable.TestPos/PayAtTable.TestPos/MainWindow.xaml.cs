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
            HandleRequest(request, () => new PATResponse { EFTPOSCommand = eftposRepository.CreateEFTPOSCommand(patRequest.EFTPOSCommand) });
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
        }

        private void HandleGetTableOrders(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Orders = ordersRepository.GetOrdersFromTable(request.Header.TableId) });
        }

        //receipt info to print
        private void HandleGetOrderReceipt(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Receipt = ordersRepository.GetCustomerReceiptFromOrderId(request.Header.OrderId, request.Header.ReceiptOptionId) });
        }


        private void HandleGetOrder(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Order = ordersRepository.GetOrder(request.Header.OrderId) });
        }

        private void HandleGetTables(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Tables = tablesRepository.GetTables() });
        }

        private void HandleGetSettings(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Settings = settingsRepository.GetSettings(myApiDataViewModel.Options) });
        }

        private void HandleGetTablesWithOrders(POSAPIMsg request)
        {
            HandleRequest(request, () => new PATResponse() { Tables = tablesRepository.GetTablesWithOrders() });
        }

        void Log(string s, ViewModels.LogType type = ViewModels.LogType.INFO)
        {
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
