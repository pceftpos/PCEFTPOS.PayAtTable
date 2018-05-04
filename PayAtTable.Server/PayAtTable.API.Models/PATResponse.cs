using System.Collections.Generic;

namespace PayAtTable.Server.Models
{
    public class PATResponse
    {
        public class ModelError
        {
            public string Field { get; set; }
            public string ErrorMessage { get; set; }
        }

        public string ResultText { get; set; } = null;
        public IEnumerable<ModelError> ModelErrors { get; set; } = null;

        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public Order Order { get; set; }
        public Receipt Receipt { get; set; }
        public EFTPOSCommand EFTPOSCommand { get; set; }
        public Tender Tender { get; set; }
        public Settings Settings { get; set; }
    }
}