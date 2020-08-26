using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;

namespace PayAtTable.Server.Data
{
    public interface ITablesRepository
    {
        IEnumerable<Table> GetTables();
        IEnumerable<Table> GetTablesWithOrders();
    }
    
    public interface IOrdersRepository
    {
        IEnumerable<Order> GetOrdersFromTable(string tableId);
        Order GetOrder(string orderId);
        Receipt GetCustomerReceiptFromOrderId(string orderId, string receiptOptionId);
    }

    public interface ITendersRepository
    {
        Tender CreateTender(Tender t);
        Tender UpdateTender(Tender t);
    }

    public interface IEFTPOSRepository
    {
        EFTPOSCommand CreateEFTPOSCommand(EFTPOSCommand c);
    }

    public class SettingsOptions
    {
        public List<TenderOption> TenderTypes { get; set; } = new List<TenderOption>();
        public bool IsMultipleTenderTypes { get; set; } = true;
        public bool IsMultiplePrintOptions { get; set; } = false;

        public bool IsTippingEnabled { get; set; } = false;

        public Location CustomReceiptLocation { get; set; } = Location.None;

        public string TxnType { get; set; } = "P";
        public string CsdReservedString2 { get; set; } = "EFTPOS";
    }

    public interface ISettingsRepository
    {
        Settings GetSettings(SettingsOptions options);
    }
}
