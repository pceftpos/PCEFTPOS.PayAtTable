using Newtonsoft.Json;
using PayAtTable.Server.Data;
using PayAtTable.Server.DemoRepository;
using PayAtTable.Server.Models;
using PCEFTPOS.API.IPInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PayAtTable.TestPos.IPInterface
{
    public class IPData : INotifyPropertyChanged
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6001;
        public bool UseSSL { get; set; } = false;
        public string ConnectedState { get; set; } = "Connect";

        public Dictionary<string, string> TxnTypes { get; set; } = new Dictionary<string, string>();
        public Server.Data.SettingsOptions Options { get; set; } = new Server.Data.SettingsOptions();

        bool _isConnected = false;
        public bool IsConnected { get { return _isConnected; } set { _isConnected = value; ConnectedState = (value) ? "Disconnect" : "Connect"; NotifyPropertyChanged(nameof(ConnectedState)); NotifyPropertyChanged(nameof(IsConnected)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public IPData()
        {
            TxnTypes.Add("P", "Purchase Cash");
            TxnTypes.Add("R", "Refund");
        }
    }

    public class EftWrapper 
    {
        IEFTClientIPAsync eft = new EFTClientIPAsync();
        ILogger _logger = new Logger();

        ITablesRepository _tablesRepository = new TablesRepositoryDemo();
        IOrdersRepository _ordersRepository = new OrdersRepositoryDemo();
        ITendersRepository _tendersRepository = new TendersRepositoryDemo();
        IEFTPOSRepository _eftposRepository = new EFTPOSRepositoryDemo();
        ISettingsRepository _settingsRepository = new SettingsRepositoryDemo();

        public IPData Data { get; set; } = new IPData();

        public ObservableCollection<LogData> Logs { get { return _logger.Logs; } }
        public LogData SelectedData { get { return _logger.SelectedData; } set { _logger.SelectedData = value; } }

        public event EventHandler OnLogUpdate;

        public EftWrapper()
        {
            eft.OnLog += Eft_OnLog;
            _logger.OnLogUpdate += _logger_OnLogUpdate;
        }

        private void _logger_OnLogUpdate(object sender, EventArgs e)
        {
            OnLogUpdate?.Invoke(this, EventArgs.Empty);
        }

        private void Eft_OnLog(object sender, LogEventArgs e)
        {
            _logger.Log(e.Message);
        }

        #region Common

        private async Task<bool> SendRequest(EFTPayAtTableRequest request)
        {
            bool result = false;
            try
            {
                _logger.Log("Sending request...");
                result = await eft.WriteRequestAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
                result = false;
            }

            return result;
        }

        private async Task ProcessRequest()
        {
            do
            {
                _logger.Log("Awaiting message...");
                var r = await eft.ReadResponseAsync();
                if (r == null) // stream is busy
                {
                    _logger.Log("Received null response. Waiting 3 secs...");
                    await Task.Delay(3000); // wait 3 sec?
                    continue;
                }

                _logger.Log($"Message received! {r}");
                if (r is EFTPayAtTableResponse)
                {
                    try
                    {
                        var response = (EFTPayAtTableResponse)r;
                        var msg = new POSAPIMsg();
                        msg.ParseFromString(response);

                        _logger.Log(new LogData(msg.ToString(), $"RX (Handling message): {msg.ToString()}"));

                        switch (msg.Header.RequestMethod)
                        {
                            case RequestMethod.Settings:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Settings = _settingsRepository.GetSettings(Data.Options) });
                                break;
                            case RequestMethod.Tables:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Tables = _tablesRepository.GetTables() });
                                break;
                            case RequestMethod.TablesWithOrders:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Tables = _tablesRepository.GetTablesWithOrders() }); 
                                break;

                            case RequestMethod.TableOrders:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Orders = _ordersRepository.GetOrdersFromTable(msg.Header.TableId) });
                                break;


                            case RequestMethod.Order:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Order = _ordersRepository.GetOrder(msg.Header.OrderId) });
                                break;

                            case RequestMethod.OrderReceipt:
                                if (msg.Header.RequestType == RequestType.GET)
                                    await HandleRequest(msg, () => new PATResponse() { Receipt = _ordersRepository.GetCustomerReceiptFromOrderId(msg.Header.OrderId, msg.Header.ReceiptOptionId) });
                                break;

                            case RequestMethod.Tender:
                                if (msg.Header.RequestType == RequestType.POST)
                                {
                                    await HandleCreateTender(msg);
                                }
                                else if (msg.Header.RequestType == RequestType.PUT)
                                    await HandleUpdateTender(msg);
                                break;

                            case RequestMethod.EFTPOSCommand:
                                if (msg.Header.RequestType == RequestType.POST) //looks like there are 3 of them to handle 
                                    await HandleEFTPOSCommand(msg);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex.Message, LogType.ERROR);
                    }
                }
                else if (r is EFTStatusResponse)
                {
                    var response = (EFTStatusResponse)r;
                    _logger.Log($"Status response. {response.ResponseCode} {response.ResponseText}", (response.Success ? LogType.INFO : LogType.ERROR));
                    _logger.Log($"Pinpad online: {response.Success}");
                }
            }
            while (true);
            
        }

        async Task HandleRequest(POSAPIMsg message, Func<PATResponse> func)
        {
            _logger.Log($"Processing {message.Header.RequestType} {message.Header.RequestMethod}");

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
                _logger.Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.BadRequest;
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.NotFound;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
                rc = ResponseCode.ServerError;
            }

            // Build response
            POSAPIMsg resp = new POSAPIMsg()
            {
                Header = new POSAPIMsgHeader()
                {
                    RequestMethod = message.Header.RequestMethod,
                    RequestType = message.Header.RequestType,
                    ResponseCode = content != null ? ResponseCode.Ok : rc,
                    ContentLength = content?.Length ?? 0
                },
                Content = content
            };

            var respString = resp.ToString();
            _logger.Log(new LogData(respString, $"TX (Building request): {respString}"));

            try
            {
                // build eft request
                EFTPayAtTableRequest eftRequest = new EFTPayAtTableRequest();
                eftRequest.Header = resp.Header.ToString();
                eftRequest.Content = resp.Content;

                await SendRequest(eftRequest);
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
                throw;
            }
        }

        public async Task GetStatus()
        {
            try
            {
                _logger.Log("Sending status request...");

                var request = new EFTStatusRequest() { StatusType = StatusType.Standard, Application = TerminalApplication.EFTPOS };
                await eft.WriteRequestAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
            }
        }

        #endregion

        #region Processing
        async Task HandleCreateTender(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);
            if (patRequest == null || patRequest.Tender == null)
            {
                _logger.Log($"Invalid Request found in: {request.Header.RequestType} {request.Header.RequestMethod}", LogType.ERROR);
                return;
            }

            await HandleRequest(request, () => new PATResponse { Tender = _tendersRepository.CreateTender(patRequest.Tender) });
        }

        async Task HandleUpdateTender(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);
            if (patRequest == null || patRequest.Tender == null)
            {
                _logger.Log($"Invalid Request found in: {request.Header.RequestType} {request.Header.RequestMethod}", LogType.ERROR);
                return;
            }

            await HandleRequest(request, () => new PATResponse { Tender = _tendersRepository.UpdateTender(patRequest.Tender) });
        }

        async Task HandleEFTPOSCommand(POSAPIMsg request)
        {
            var patRequest = JsonConvert.DeserializeObject<PATRequest>(request.Content);

            await HandleRequest(request, () => new PATResponse { EFTPOSCommand = _eftposRepository.CreateEFTPOSCommand(patRequest.EFTPOSCommand) });
        }
        #endregion


        #region Connect

        public async Task<bool> Connect()
        {
            try
            {
                if (await eft.ConnectAsync(Data.Host, Data.Port, Data.UseSSL))
                {
                    Data.IsConnected = true;
                    await ProcessRequest();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
            }
            return false;
        }

        public void Disconnect()
        {
            try
            {
                if (!eft.DisconnectAsync())
                    return;

                Data.IsConnected = false;
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, LogType.ERROR);
            }
        }

        #endregion

    }
}
