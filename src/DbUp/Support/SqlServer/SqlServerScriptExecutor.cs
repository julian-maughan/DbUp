using System;
using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Support.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerScriptExecutor : DbScriptExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionManagerFactory"></param>
        /// <param name="log"></param>
        /// <param name="schema"></param>
        /// <param name="variablesEnabled"></param>
        /// <param name="scriptPreprocessors"></param>
        public SqlServerScriptExecutor(Func<IConnectionManager> connectionManagerFactory, Func<IUpgradeLog> log, string schema, Func<bool> variablesEnabled, IEnumerable<IScriptPreprocessor> scriptPreprocessors)
            : base(connectionManagerFactory, log, schema, variablesEnabled, scriptPreprocessors, new SqlServerObjectNameParser())
        {
        }

        protected override string VerifySchemaSql(string schema)
        {
            return string.Format(@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{0}') Exec('CREATE SCHEMA [{0}]')", schema);
        }
    }
}