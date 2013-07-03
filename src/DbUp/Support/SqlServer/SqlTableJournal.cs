using System;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Support.SqlServer
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for a 
    /// SQL Server database using a table called dbo.SchemaVersions.
    /// </summary>
    public class SqlTableJournal : DbTableJournal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionManager"></param>
        /// <param name="logger"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        public SqlTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string table)
            : base(connectionManager, logger, new SqlServerObjectNameParser(), schema, table)
        {
        }

        /// <summary>
        /// The sql to exectute to create the schema versions table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected override string CreateTableSql(string tableName)
        {
            return string.Format(@"create table {0} (
	[Id] int identity(1,1) not null constraint PK_SchemaVersions_Id primary key,
	[ScriptName] nvarchar(255) not null,
	[Applied] datetime not null
)", tableName);
        }
    }
}