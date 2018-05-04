using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;


namespace PayAtTable.Server.DemoRepository
{
    public class OrdersRepositoryDemo: IOrdersRepository
    {
        protected static readonly Common.Logging.ILog log = 
                Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void TestLog()
        {
            log.InfoEx("Log test...");
        }

        public IEnumerable<Models.Order> GetOrdersFromTable(string tableId)
        {
            log.InfoEx("cpk Order from Table ID...");
            var orders = from o in SampleData.Current.Orders
                         where o.TableId.Equals(tableId)
                         select o;
            return orders;
        }

        public Models.Order GetOrder(string orderId)
        {
            var o = SampleData.Current.Orders.Find((order) => order.Id.Equals(orderId));
            log.InfoEx("cpk Trying to find order...");
            if (o == null)
                throw new ResourceNotFoundException(String.Format("Order id {0} not found", orderId));

            return o;
        }

        public Models.Receipt GetCustomerReceiptFromOrderId(string orderId, string receiptOptionId)
        {
            // Find the order
            var o = SampleData.Current.Orders.Find((order) => order.Id.Equals(orderId));
            if (o == null)
                throw new ResourceNotFoundException(String.Format("Order id {0} not found", orderId));

            var lines = new List<string>();
            var t = SampleData.Current.Tables.Find(table => table.Id.Equals(o.TableId));

            //id 99 is reserved for custom header/footer POS receipt
            if (receiptOptionId == "99")
            {
                lines.Add("------------------------");
                lines.Add("THIS IS A RECEIPT");
                lines.Add("WITH POS DATA");
                lines.Add("IN IT");
                return new Receipt { Lines = lines };
            }

            // Find the table for this order
            lines.Add("------------------------");
            lines.Add("-- Test Data --");
            lines.Add(String.Format("{0, 24}", (t != null) ? t.DisplayName : "UNKNOWN"));
            lines.Add(String.Format("ORDER#{0, 18}", o.Id));
            lines.Add(String.Format("{0, 24}", o.DisplayName));
            lines.Add(String.Format("OWING{0,19:C2}", o.AmountOwing));
            lines.Add("------------------------");

            if (receiptOptionId == "1")
            {
                lines.Add(string.Format("{0, 24}", "ADVERTISEMENT"));
                lines.Add(string.Format("{0, 24}", "WIN $5M GRAND PRIZE"));
                lines.Add(string.Format("{0, 24}", "join now!"));
                lines.Add(string.Format("{0, 24}", "see posters for details"));
                lines.Add("------------------------");
            }

            return new Receipt { Lines = lines };
        }



    }
}
