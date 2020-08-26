using System;

namespace PayAtTable.Server.Models
{
    public enum EFTPOSCommandType { UpdateConfig = 0, DoTransaction = 100, DoLogon = 101, TransactionEvent = 200, LogonEvent = 201, DoKeyPress = 300, DisplayEvent = 400, PrintEvent = 401 }

    /// <summary>
    /// The current state of the command. New commands have a state of AwaitingDeviceAck. 
    /// One the command has been picked up by a device the state changes to AwaitingDeviceResponse.
    /// Once complete the state is changed to CompletedSuccessful or CompletedUnsuccessful
    /// </summary>
    public enum EFTPOSCommandState { AwaitingDeviceAck = 0, AwaitingDeviceResponse = 10, CompletedSuccessful = 20, CompletedUnsuccessful = 30 }

    public class EFTPOSCommandResult
    {
        public String ResponseCode { get; set; }
        public String ResponseText { get; set; }
        public bool Success { get; set; }
        public EFTPOSCommandState State { get; set; }

        public EFTPOSCommandResult(string responseCode, string responseText, bool success, EFTPOSCommandState state)
        {
            ResponseCode = responseCode;
            ResponseText = responseText;
            Success = success;
            State = state;
        }


        // 
        public static EFTPOSCommandResult ERC_00_Approved = new EFTPOSCommandResult("00", "APPROVED", false, EFTPOSCommandState.CompletedSuccessful);
        public static EFTPOSCommandResult ERC_PG_ClientOffline = new EFTPOSCommandResult("PG", "CLIENT OFFLINE", false, EFTPOSCommandState.CompletedUnsuccessful);
        public static EFTPOSCommandResult ERC_XB_CommandInProgress = new EFTPOSCommandResult("XB", "COMMAND IN PROGRESS", false, EFTPOSCommandState.CompletedUnsuccessful);
        public static EFTPOSCommandResult ERC_XC_EFTTimeout = new EFTPOSCommandResult("XC", "EFT TIMEOUT", false, EFTPOSCommandState.CompletedUnsuccessful);
        public static EFTPOSCommandResult ERC_XX_UnknownError = new EFTPOSCommandResult("XX", "UNKNOWN ERROR", false, EFTPOSCommandState.CompletedUnsuccessful);
    }

    /// <summary>
    /// Pretty much maps to an EFT-Client request 
    /// </summary>
    public class EFTPOSCommand
    {
        public string Id { get; set; }

        /// <summary>
        /// The device this command is for
        /// </summary>
        public string EFTPOSId { get; set; }

        /// <summary>
        /// Command type
        /// </summary>
        public EFTPOSCommandType EFTPOSCommandType { get; set; }

        /// <summary>
        /// Current command state
        /// </summary>
        public int? EFTPOSCommandState { get; set; }

        /// <summary>
        /// The Tender this command is linked to
        /// </summary>
        public string TenderId { get; set; }

        /// <summary>
        /// The key of the initial request if this is an event
        /// </summary>
        public string OriginalEFTPOSCommandId { get; set; }

        /// <summary>
        /// Timestamp for when the command was picked up 
        /// </summary>
        public DateTime CommandStartDateTime { get; set; }

        /// <summary>
        /// Time stamp for when the command was completed
        /// </summary>
        public DateTime CommandEndDateTime { get; set; }

        /// <summary>
        /// Time stamp for when this command should no longer be handled
        /// </summary>
        public DateTime CommandStaleDateTime { get; set; }

        /// <summary>
        /// Timestamp for when this record was created
        /// </summary>
        public DateTime InsertionDateTime { get; set; }

        // Fields from PC-EFTPOS
        public string AccountType { get; set; }
        public decimal AmtCash { get; set; }
        public decimal AmtPurchase { get; set; }
        public decimal AmtTip { get; set; }
        public decimal AmtTotal { get; set; }
        public string Application { get; set; }
        public string AuthCode { get; set; }
        public string Caid { get; set; }
        public string Catid { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
        public string CsdReservedString1 { get; set; }
        public string CsdReservedString2 { get; set; }
        public string CsdReservedString3 { get; set; }
        public string CsdReservedString4 { get; set; }
        public string CsdReservedString5 { get; set; }
        public bool CsdReservedBool1 { get; set; }
        public bool CutReceipt { get; set; }
        public string CurrencyCode { get; set; }
        public string DataField { get; set; }
        public string Date { get; set; }
        public string DateExpiry { get; set; }
        public string DateSettlement { get; set; }
        public string DialogPosition { get; set; }
        public string DialogTitle { get; set; }
        public string DialogType { get; set; }
        public int DialogX { get; set; }
        public int DialogY { get; set; }
        public bool EnableTip { get; set; }
        public bool EnableTopmost { get; set; }
        public string Merchant { get; set; }
        public string MessageType { get; set; }
        public char PanSource { get; set; }
        public string Pan { get; set; }
        public string PosProductId { get; set; }
        public string PurchaseAnalysisData { get; set; }
        public bool ReceiptAutoPrint { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string ResponseType { get; set; }
        public string Rrn { get; set; }
        public bool Success { get; set; }
        public string STAN { get; set; }
        public string Time { get; set; }
        public string TxnRef { get; set; }
        public string TxnType { get; set; }
        public string Track1 { get; set; }
        public string Track2 { get; set; }
        

        public EFTPOSCommand()
        {
            Reset();
        }

        public void Reset()
        {
            AccountType = "";
            AmtCash = 0.0M;
            AmtPurchase = 0.0M;
            AmtTip = 0.0M;
            AmtTotal = 0.0M;
            Application = "";
            AuthCode = "";
            Caid = "";
            Catid = "";
            CardName = "";
            CardType = "";
            CsdReservedString1 = "";
            CsdReservedString2 = "";
            CsdReservedString3 = "";
            CsdReservedString4 = "";
            CsdReservedString5 = "";
            CsdReservedBool1 = false;
            CutReceipt = false;
            CurrencyCode = "";
            DataField = "";
            Date = "";
            DateExpiry = "";
            DateSettlement = "";
            DialogPosition = "";
            DialogTitle = "";
            DialogType = "";
            DialogX = 0;
            DialogY = 0;
            EnableTip = false;
            EnableTopmost = false;
            Merchant = "";
            MessageType = "";
            PanSource = ' ';
            Pan = "";
            PosProductId = "";
            PurchaseAnalysisData = "";
            ReceiptAutoPrint = false;
            ResponseCode = "";
            ResponseText = "";
            ResponseType = "";
            Rrn = "";
            Success = false;
            STAN = "";
            Time = "";
            TxnRef = "";
            TxnType = "";
            Track1 = "";
            Track2 = "";
        }

        public void SetResult(EFTPOSCommandResult r)
        {
            ResponseCode = r.ResponseCode;
            ResponseText = r.ResponseText;
            Success = r.Success;
            EFTPOSCommandState = (int)r.State;
        }

        public EFTPOSCommand DeepCopy()
        {
            return (EFTPOSCommand)this.MemberwiseClone();
        }
    }
}