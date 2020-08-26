namespace PayAtTable.Server.Models
{
    public enum TenderState { Pending = 0, CompleteSuccessful = 1, CompleteUnsuccessful = 2 }

    public class Tender
    {
        /// <summary>
        /// Unique identifier for this tender
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The orderId this tender is attached to
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// The state of the tender
        /// </summary>
        public TenderState TenderState { get; set; }

        /// <summary>
        /// The id of the tender option which was selected to create this tender
        /// </summary>
        public string TenderOptionId { get; set; }

        /// <summary>
        /// The final purchase amount of this tender
        /// </summary>
        public decimal AmountPurchase { get; set; }

        /// <summary>
        /// The original purchase amount of this tender
        /// </summary>
        public decimal OriginalAmountPurchase { get; set; }
    }
}