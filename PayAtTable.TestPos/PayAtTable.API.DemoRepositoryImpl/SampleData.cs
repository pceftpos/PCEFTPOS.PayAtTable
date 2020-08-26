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
    public class SampleData
    {
        private static readonly SampleData _current = new SampleData(true);

        public List<Table> Tables { get; set; }
        public List<Order> Orders { get; set; }
        public List<Tender> Tenders { get; set; }
        public List<EFTPOSCommand> EFTPOSCommands { get; set; }
        public int LastTenderId { get; set; }
        public int LastEFTPOSCommandId { get; set; }

        public static SampleData Current
        {
            get
            {
                return _current;
            }
        }

        private SampleData()
        {
            Tables = new List<Table>();
            Orders = new List<Order>();
            Tenders = new List<Tender>();
            EFTPOSCommands = new List<EFTPOSCommand>();
            LastTenderId = 0;
            LastEFTPOSCommandId = 0;
        }

        private SampleData(bool populate)
        {
            Tables = new List<Table>();
            Orders = new List<Order>();
            Tenders = new List<Tender>();
            EFTPOSCommands = new List<EFTPOSCommand>();
            LastTenderId = 0;
            LastEFTPOSCommandId = 0;


            Tables.Add(new Table() { Id = "50", DisplayNumber = 1, DisplayName = "TABLE 1", ServerName = "Caitlyn" });
            Tables.Add(new Table() { Id = "51", DisplayNumber = 2, DisplayName = "TABLE 2", ServerName = "Sonya" });
            Tables.Add(new Table() { Id = "52", DisplayNumber = 3, DisplayName = "TABLE 3", ServerName = "Fail" });
            Tables.Add(new Table() { Id = "53", DisplayNumber = 4, DisplayName = "TABLE 4", ServerName = "Donatello" });
            Tables.Add(new Table() { Id = "54", DisplayNumber = 5, DisplayName = "TABLE 5", ServerName = "Michelangelo" });
            Tables.Add(new Table() { Id = "55", DisplayNumber = 11, DisplayName = "TABLE 11" });
            Tables.Add(new Table() { Id = "56", DisplayNumber = 12, DisplayName = "TABLE 12" });
            Tables.Add(new Table() { Id = "57", DisplayNumber = 13, DisplayName = "TABLE 13" });
            Tables.Add(new Table() { Id = "58", DisplayNumber = 14, DisplayName = "TABLE 14" });
            //adding 3 orders to process table 1
            Orders.Add(new Order() { Id = "101", TableId = "50", AmountOwing = 100.00M, DisplayName = "Sven" });
            Orders.Add(new Order() { Id = "109", TableId = "50", AmountOwing = 1.00M, DisplayName = "Duke" });
            Orders.Add(new Order() { Id = "110", TableId = "50", AmountOwing = 2.00M, DisplayName = "Elsa" });

            // 6 buttons or 3 buttons display
            Orders.Add(new Order() { Id = "102", TableId = "51", AmountOwing = 10.00M, DisplayName = "Anna" });
            Orders.Add(new Order() { Id = "111", TableId = "51", AmountOwing = 15.00M, DisplayName = "Eugene" });
            Orders.Add(new Order() { Id = "112", TableId = "51", AmountOwing = 20.00M, DisplayName = "Alfred" });
            Orders.Add(new Order() { Id = "113", TableId = "51", AmountOwing = 25.00M, DisplayName = "Vincent" });
            Orders.Add(new Order() { Id = "114", TableId = "51", AmountOwing = 30.00M, DisplayName = "Denise" });

            Orders.Add(new Order() { Id = "103", TableId = "52", AmountOwing = 3.33M, DisplayName = "Fail" });
            Orders.Add(new Order() { Id = "104", TableId = "53", AmountOwing = 20.00M, DisplayName = "Kristoff" });
            Orders.Add(new Order() { Id = "105", TableId = "55", AmountOwing = 15.00M, DisplayName = "Hans" });
            Orders.Add(new Order() { Id = "106", TableId = "56", AmountOwing = 96.00M, DisplayName = "Duke" });
            Orders.Add(new Order() { Id = "107", TableId = "57", AmountOwing = 52.00M, DisplayName = "Oaken" });
            
            //table without order
            //Orders.Add(new Order() { Id = "108", TableId = "58", AmountOwing = 756.00M, DisplayName = "Gerda" });
        }

        public void Set(SampleData data)
        {
            _current.EFTPOSCommands = data.EFTPOSCommands;
            _current.LastEFTPOSCommandId = data.LastEFTPOSCommandId;
            _current.LastTenderId = data.LastTenderId;
            _current.Orders = data.Orders;
            _current.Tables = data.Tables;
            _current.Tenders = data.Tenders;
        }

        public void Clear()
        {
            _current.Tables.Clear();
            _current.Orders.Clear();
            _current.Tenders.Clear();
            _current.EFTPOSCommands.Clear();
            _current.LastTenderId = -1;
            _current.LastEFTPOSCommandId = -1;
        }
    }
}
