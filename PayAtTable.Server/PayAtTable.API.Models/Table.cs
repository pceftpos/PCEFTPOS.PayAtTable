namespace PayAtTable.Server.Models
{
    public class Table
    {
        /// <summary>
        /// Unique identifier for this table
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A name to be displayed to a user e.g. "Table 42"
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A number for the user to select this order lookup e.g. 42
        /// </summary>
        public int DisplayNumber { get; set; }
    }
}