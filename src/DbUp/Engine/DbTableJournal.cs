using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;
using DbUp.Support.SqlServer;

namespace DbUp.Engine
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for a 
    /// SQL Server database using a table called dbo.SchemaVersions.
    /// </summary>
    public abstract class DbTableJournal : IJournal
    {
        private readonly string schemaTableName;
        private readonly Func<IConnectionManager> connectionManager;
        private readonly Func<IUpgradeLog> log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTableJournal"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        /// <param name="logger">The log.</param>
        /// <param name="objectNameParser"></param>
        /// <param name="schema">The schema that contains the table.</param>
        /// <param name="table">The table name.</param>
        protected DbTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, IObjectNameParser objectNameParser, string schema, string table)
        {
            schemaTableName = objectNameParser.Quote(table);

            if (!string.IsNullOrEmpty(schema))
                schemaTableName = objectNameParser.Quote(schema) + "." + objectNameParser.Quote(table);

            this.connectionManager = connectionManager;
            log = logger;
        }

        /// <summary>
        /// Recalls the version number of the database.
        /// </summary>
        /// <returns>All executed scripts.</returns>
        public string[] GetExecutedScripts()
        {
            log().WriteInformation("Fetching list of already executed scripts.");
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(String.Format("The {0} table could not be found. The database is assumed to be at version 0.", schemaTableName));
                return new string[0];
            }

            var scripts = new List<string>();
            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = GetExecutedScriptsSql(schemaTableName);
                        command.CommandType = CommandType.Text;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                scripts.Add((string)reader[0]);
                        }
                    }
                });

            return scripts.ToArray();
        }

        /// <summary>
        /// Records a database upgrade for a database specified in a given connection string.
        /// </summary>
        /// <param name="script">The script.</param>
        public void StoreExecutedScript(SqlScript script)
        {
            var exists = DoesTableExist();
            if (!exists)
            {
                log().WriteInformation(String.Format("Creating the {0} table", schemaTableName));

                connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                    {
                        using (var command = dbCommandFactory())
                        {
                            command.CommandText = CreateTableSql(schemaTableName);
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }

                        log().WriteInformation(String.Format("The {0} table has been created", schemaTableName));
                    });
            }

            connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = GetStoreExecutedScriptsSql(schemaTableName, script.Name, DateTime.UtcNow);
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                });
        }

        protected abstract string CreateTableSql(string tableName);

        protected virtual string GetStoreExecutedScriptsSql(string tableName, string scriptName, DateTime when)
        {
            return string.Format("insert into {0} (ScriptName, Applied) values ('{1}', '{2}')", tableName, scriptName, when.ToString("s"));
        }

        protected virtual string GetExecutedScriptsSql(string table)
        {
            return string.Format("select [ScriptName] from {0} order by [ScriptName]", table);
        }

        protected virtual string GetDoesTableExistSql(string tableName)
        {
            return string.Format("select count(*) from {0}", tableName);
        }

        private bool DoesTableExist()
        {
            return connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
            {
                try
                {
                    using (var command = dbCommandFactory())
                    {
                        command.CommandText = GetDoesTableExistSql(schemaTableName);
                        command.CommandType = CommandType.Text;
                        command.ExecuteScalar();
                        return true;
                    }
                }
                catch (DbException)
                {
                    return false;
                }
            });
        }
    }
}