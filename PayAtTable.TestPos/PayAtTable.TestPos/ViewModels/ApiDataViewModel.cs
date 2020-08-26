using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayAtTable.Server.DemoRepository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PayAtTable.TestPos.ViewModels
{
    public enum LogType { INFO, WARNING, ERROR }

    public class LogData
    {
        public LogData(string rawData, string data, LogType type = LogType.INFO)
        {
            RawData = rawData;
            Data = data;
            Type = type;
        }

        string _rawData = string.Empty;
        public string RawData
        {
            get { return _rawData; }
            set
            {
                _rawData = value;

                try
                {
                    var temp = new POSAPIMsg();
                    temp.ParseFromString(value);

                    var prefix = temp.Header.ToString().Substring(0, 6) + Environment.NewLine;

                    var tempHeader = JsonConvert.DeserializeObject(temp.Header.ToString().Remove(0, 6));
                    var header = JsonConvert.SerializeObject(tempHeader, Formatting.Indented);

                    var tempContent = JsonConvert.DeserializeObject(temp.Content);
                    var content = JsonConvert.SerializeObject(tempContent, Formatting.Indented);

                    _rawData = prefix + header + Environment.NewLine + content;
                }
                catch (Exception)
                {
                    _rawData = value;
                }
            }
        }
        public string Data { get; set; }
        public LogType Type { get; set; }
    }

    public class OrderData : INotifyPropertyChanged
    {
        public string OrderId { get; set; } = string.Empty;
        public string OrderName { get; set; } = string.Empty;

        decimal _cost = 0;
        public decimal OrderCost { set { _cost = value; Notify(nameof(OrderCost)); } get { return _cost; } }

        string _status = string.Empty;
        public string OrderStatus { set { _status = value; Notify(nameof(OrderStatus)); } get { return _status; } }

        bool _processing = false;
        public bool OrderProcessing { set { _processing = value; Notify(nameof(OrderProcessing)); } get { return _processing; } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class TableData
    {
        public string Id { set; get; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public ObservableCollection<OrderData> OrderList { get; set; } = new ObservableCollection<OrderData>();
    }

    public class ApiDataViewModel : INotifyPropertyChanged
    {
        const string SETTINGS_FILENAME = "settings.json";
        const string DATA_FILENAME = "data.json";

        public ObservableCollection<LogData> Logs { get; set; } = new ObservableCollection<LogData>();

        public LogData SelectedData { get; set; } = null;

        public Dictionary<string, string> TxnTypes { get; set; } = new Dictionary<string, string>();

        public Server.Data.SettingsOptions Options { get; set; } = new Server.Data.SettingsOptions();

        ObservableCollection<TableData> _tableList = new ObservableCollection<TableData>();
        public ObservableCollection<TableData> TableList { get { return _tableList; } set { _tableList = value; Notify(nameof(TableList)); } }

        private void Notify(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        int _tableIndex = -1;
        public int CurrentTableIndex { get { return _tableIndex; } set { _tableIndex = value; Notify(nameof(CurrentTableIndex)); } }
        string _previousTable = string.Empty;
        string _currentTable = string.Empty;
        public string CurrentTable { private get { return _currentTable; } set { _previousTable = _currentTable; _currentTable = value; GetTableIndex(); } }  
        string _currentOrder = string.Empty;
        string _previousOrder = string.Empty;
        public string CurrentOrder { private get { return _currentOrder; } set { _previousOrder = _currentOrder; _currentOrder = value; _processingOrder = true; } }

        /// <summary>
        /// Enable this to test restoring of last transaction data
        /// </summary>
        public bool RestoreData { get; private set; } = false;

        private bool _processingOrder = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ApiDataViewModel()
        {
            TxnTypes.Add("P", "Purchase Cash");
            TxnTypes.Add("R", "Refund");

            LoadSettings();
            LoadData();
        }

        public void Log(string message, LogType type = LogType.INFO)
        {
            var msg = $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}: [{type}] {message}";
            var x = new LogData(message, msg, type);
            Logs.Add(x);
        }

        public void Log(LogData data)
        {
            data.Data = $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}: [{data.Type}] {data.Data}";
            Logs.Add(data);
        }

        public void CreateTableList()
        {
            try
            {
                SampleData.Current.Tables.ForEach(t =>
                {
                    var orders = new ObservableCollection<OrderData>();

                    SampleData.Current.Orders.FindAll(o => o.TableId.Equals(t.Id)).ForEach(item =>
                    {
                        orders.Add(new OrderData() { OrderId = item.Id, OrderCost = item.AmountOwing, OrderStatus = item.OrderState.ToString(), OrderName = item.DisplayName });
                    });

                    if (orders.Count > 0)
                    {
                        TableList.Add(new TableData { Id = t.Id, OrderList = orders, TableName = t.DisplayName });
                    }
                });

            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        public void UpdateOrderStatus()
        {
            OrderData orderV = null;
            OrderData prevOrderV = null;

            try
            {
                orderV = TableList.First(x => x.Id.Equals(CurrentTable)).OrderList.First(o => o.OrderId.Equals(CurrentOrder));
            }
            catch
            {
            }

            try
            {
                prevOrderV = TableList.First(x => x.Id.Equals(_previousTable)).OrderList.First(o => o.OrderId.Equals(_previousOrder));
            }
            catch
            {
            }

            if (orderV != null)
                orderV.OrderProcessing = _processingOrder;

            if( prevOrderV != null)
                prevOrderV.OrderProcessing = false;
        }

        public void UpdateTableData()
        {
            try
            {


                var tableV = TableList.First(x => x.Id.Equals(CurrentTable));
                var orderListV = tableV?.OrderList;
                var orderV = orderListV.First(o => o.OrderId.Equals(CurrentOrder));

                var order = SampleData.Current.Orders.Find(x => x.Id.Equals(CurrentOrder));
                if (order.OrderState == Server.Models.OrderState.Complete)
                {
                    if (orderListV != null)
                    {
                        orderListV.Remove(orderV);

                        if (orderListV.Count == 0)
                            TableList.Remove(tableV);
                    }
                }
                else
                {
                    if (orderV != null)
                    {
                        orderV.OrderStatus = order.OrderState.ToString();
                        orderV.OrderCost = order.AmountOwing;
                    }
                }

                _processingOrder = false;
                UpdateOrderStatus();
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void GetTableIndex()
        {
            try
            {
                var table = TableList.First(x => x.Id.Equals(CurrentTable));
                if (table != null)
                {
                    CurrentTableIndex = TableList.IndexOf(table);
                    UpdateOrderStatus();
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        public void LoadSettings()
        {
            try
            {
                Server.Data.SettingsOptions data = new Server.Data.SettingsOptions();

                JsonWriter settings = new JsonWriter();
                settings.Load(SETTINGS_FILENAME, out data);

                if (data != default(Server.Data.SettingsOptions))
                {
                    Options = data;
                }
            }
            catch
            {
            }
        }

        public void SaveSettings()
        {
            try
            {
                JsonWriter settings = new JsonWriter();
                settings.Save(Options, SETTINGS_FILENAME);
            }
            catch 
            {
            }
        }

        public void LoadData()
        {
            if (!RestoreData)
                return;

            try
            {
                var data = SampleData.Current;
                data.Clear();

                var w = new JsonWriter();
                w.Load(DATA_FILENAME, out data);

                if (data != default(SampleData))
                {
                    SampleData.Current.Set(data);
                }
            }
            catch
            {
            }
        }

        public void SaveData()
        {
            if (!RestoreData)
                return;

            try
            {
                var data = new JsonWriter();
                data.Save(SampleData.Current, DATA_FILENAME);
            }
            catch
            {
            }
        }
    }
}
