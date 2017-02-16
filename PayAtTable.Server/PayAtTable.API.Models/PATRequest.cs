using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayAtTable.Server.Models
{
    public class PATRequest
    {
        public Tender Tender { get; set; }
        public EFTPOSCommand EFTPOSCommand { get; set; }
    }
}