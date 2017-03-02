using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayAtTable.TestPos.IPInterface
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

    public interface ILogger
    {
        void Log(LogData data);
        void Log(string message, LogType type = LogType.INFO);

        ObservableCollection<LogData> Logs { get; set; }
        LogData SelectedData { get; set; }

        event EventHandler OnLogUpdate;
    }

    public class Logger : ILogger
    {
        public ObservableCollection<LogData> Logs { get; set; } = new ObservableCollection<LogData>();

        public LogData SelectedData { get; set; } = null;

        public event EventHandler OnLogUpdate;

        public void Log(string message, LogType type = LogType.INFO)
        {
            var msg = $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}: [{type}] {message}";
            var x = new LogData(message, msg, type);
            Logs.Add(x);
            OnLogUpdate?.Invoke(this, EventArgs.Empty);
        }

        public void Log(LogData data)
        {
            data.Data = $"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}: [{data.Type}] {data.Data}";
            Logs.Add(data);
            OnLogUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
}
