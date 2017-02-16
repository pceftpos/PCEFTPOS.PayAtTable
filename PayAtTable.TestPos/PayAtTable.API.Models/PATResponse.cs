using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayAtTable.Server.Models
{
    public class PATResponse
    {
        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<Order> Orders { get; set; }

      //  public IEnumerable<TableOrders> Orders { get; set; }

        public Order Order { get; set; }
        public Receipt Receipt { get; set; }
        public EFTPOSCommand EFTPOSCommand { get; set; }
        public Tender Tender { get; set; }
        public Settings Settings { get; set; }
    }
}