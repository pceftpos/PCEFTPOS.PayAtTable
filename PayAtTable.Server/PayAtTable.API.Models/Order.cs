namespace PayAtTable.Server.Models
{
    public enum OrderState { Pending = 0, Active = 10, Tendering = 20, Complete = 30 }

    public class Order
    {
        /// <summary>
        /// Unique identifier for this order
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Order display name. e.g. the customer name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The state of this order
        /// </summary>
        public OrderState OrderState { get; set; }

        /// <summary>
        /// Amount owing on this order
        /// </summary>
        public decimal AmountOwing { get; set; }

        /// <summary>
        /// The table attached to this order
        /// </summary>
        public string TableId { get; set; }
    }
}