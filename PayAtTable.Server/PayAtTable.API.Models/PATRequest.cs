namespace PayAtTable.Server.Models
{
    public class PATRequest
    {
        public Tender Tender { get; set; }
        public EFTPOSCommand EFTPOSCommand { get; set; }
    }
}