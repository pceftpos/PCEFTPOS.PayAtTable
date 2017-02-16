using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayAtTable.Server.Models
{
    public enum TenderType { EFTPOS = 0 }

    public class TenderOption
    {
        /// <summary>
        ///  
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of this tender option
        /// </summary>
        public TenderType TenderType { get; set; }
        
        /// <summary>
        /// The merchant code used in the request for this payment option
        /// </summary>
        public string Merchant { get; set; }
        
        /// <summary>
        /// A name which can be presented to the user to identify this tender option
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// True if the user should be able to tender for an amount less than the total of the order
        /// </summary>
        public bool EnableSplitTender { get; set; }
    }


    public enum ReceiptType { Order = 0 }

    public class ReceiptOption
    {
        /// <summary>
        /// A unique identifier for this receipt option. This is passed back to the server when a receipt is requested
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of this receipt option
        /// </summary>
        public ReceiptType ReceiptType { get; set; }

        /// <summary>
        /// A name which can be presented to the user to identify this receipt option
        /// </summary>
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// The Pay at Table client settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// A list of tender options supported by the pay at table client
        /// </summary>
        public List<TenderOption> TenderOptions { get; set; }

        /// <summary>
        /// A list of receipt options supported by the pay at table client
        /// </summary>
        public List<ReceiptOption> ReceiptOptions { get; set; }

        public string CsdReservedString2 { get; set; }
        public string TxnType { get; set; }

        public bool IsTippingEnabled { get; set; }
    }
}