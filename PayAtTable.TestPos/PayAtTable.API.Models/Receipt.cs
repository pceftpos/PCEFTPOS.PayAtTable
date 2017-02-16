using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayAtTable.Server.Models
{
    public class Receipt
    {
        List<string> _lines;
        public Receipt()
        {
            _lines = new List<string>();
        }

        public List<string> Lines { get { return _lines; } set { _lines = value; } }
    }
}