using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayAtTable.Server.Data;
using PayAtTable.Server.Models;
using PayAtTable.API.Helpers;


namespace PayAtTable.Server.DemoRepository
{
    public class EFTPOSRepositoryDemo: IEFTPOSRepository
    {
        protected static readonly Common.Logging.ILog log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Models.EFTPOSCommand CreateEFTPOSCommand(Models.EFTPOSCommand eftposCommand)
        {
            // Write EFTPOS command to the database
            eftposCommand.Id = (SampleData.Current.LastEFTPOSCommandId++).ToString();
            SampleData.Current.EFTPOSCommands.Add(eftposCommand);
            return eftposCommand;
        }
    }
}
