using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using PayAtTable.Server.Properties;

namespace PayAtTable.API.Helpers
{
    public class DbFactory
    {
        /// <summary>
        /// Creates a new SqlConnection for the POS database
        /// </summary>
        /// <returns></returns>
        public static System.Data.SqlClient.SqlConnection CreatePosDb()
        {
            return null;
            //string connectionString = Settings.Default.DbConnectionString;
            //return new System.Data.SqlClient.SqlConnection(connectionString);
        }
    }
}