using PayAtTable.Server.Models;
using System.Collections.Generic;

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
