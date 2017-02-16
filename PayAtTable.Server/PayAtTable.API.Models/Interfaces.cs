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

    public interface ISettingsRepository
    {
        Settings GetSettings();
    }
}
