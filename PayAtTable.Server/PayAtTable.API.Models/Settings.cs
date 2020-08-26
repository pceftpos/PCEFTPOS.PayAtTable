using System.Collections.Generic;

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

        /// <summary>
        /// False to disable amount entry
        /// </summary>
        public bool EnableAmountEntry { get; set; }

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

    //Location of where to print the custom receipt->set to None for no custom receipt
    public enum Location { Header = 0, Footer = 1, None = -1 };

    //Who will handle the header/footer receipt
    public enum PrinterMode { PCEFTPOS = 0, POS = 1, STATIC = 2 };


    /// <summary>
    /// The printerOption for the custom header/footer receipt
    /// </summary>
    public class PrinterOption
    {
        /// <summary>
        /// Who will handle the header/footer receipt printing, default PCEFTPOS
        /// </summary>
        public PrinterMode PrintMode { get; set; }

        /// <summary>
        /// Where the custom receipt will print as a header prior to eftpos receipt or after as a footer
        /// Default: 'None' -> don't print out a custom receipt
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// If you are using PrintMode::STATIC you can set the receipt here
        /// </summary>
        public List<string> StaticReceipt { get; set; }
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
        
        /// <summary>
        /// PrinterOptions settings for custom header/footer receipts
        /// </summary>
        public PrinterOption PrinterOption { get; set; }

        public string CsdReservedString2 { get; set; }
        public string TxnType { get; set; }

        public bool IsTippingEnabled { get; set; }
    }
}