using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DbUp.Engine.Transactions;
using Oracle.DataAccess.Client;

namespace DbUp.Oracle
{
    public class OracleConnectionManager : DatabaseConnectionManager
    {
        public OracleConnectionManager(string connectionString) : base(connectionString)
        {
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        /// <summary>
        /// Statement separator is /
        /// </summary>
        public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            return Regex.Split(scriptContents, "^/\r*$", RegexOptions.Multiline)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Select(x => x.Replace("\r\n", "\n"));
        }
    }
}