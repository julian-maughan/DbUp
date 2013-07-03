using System;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;

namespace DbUp.Oracle
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for an
    /// Oracle database using a table called DBUP.SCHEMA_VERSIONS.
    /// </summary>
    public sealed class OracleTableJournal : DbTableJournal
    {
        /// <summary>
        /// The default schema for the journal table
        /// </summary>
        public const string DefaultSchema = "DBUP";

        /// <summary>
        /// The default journal table name
        /// </summary>
        public const string DefaultTableName = "SCHEMA_VERSIONS";

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTableJournal"/> class.
        /// </summary>
        public OracleTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger)
            : base(connectionManager, logger, new OracleObjectNameParser(), DefaultSchema, DefaultTableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTableJournal"/> class.
        /// </summary>
        public OracleTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
            : base(connectionManager, logger, new OracleObjectNameParser(), schema, table)
        {
        }

        protected override string CreateTableSql(string tableName)
        {
            return string.Format("CREATE TABLE {0} (ID VARCHAR2(32) DEFAULT sys_guid() NOT NULL PRIMARY KEY, SCRIPT_NAME VARCHAR2(255) NOT NULL, APPLIED DATE NOT NULL)", tableName);
        }

        protected override string GetStoreExecutedScriptsSql(string tableName, string scriptName, DateTime when)
        {
            return string.Format("INSERT INTO {0} (SCRIPT_NAME, APPLIED) VALUES ('{1}', TO_DATE('{2:yyyy-MM-dd hh:mm:ss}', 'yyyy-mm-dd hh24:mi:ss'))", tableName, scriptName, when);
        }
    }
}