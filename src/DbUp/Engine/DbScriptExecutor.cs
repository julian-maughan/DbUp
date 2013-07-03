using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DbUp.Engine.Output;
using DbUp.Engine.Preprocessors;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using DbUp.Support;

namespace DbUp.Engine
{
    /// <summary>
    /// A standard implementation of the IScriptExecutor interface that executes against a database.
    /// </summary>
    public abstract class DbScriptExecutor : IScriptExecutor
    {
        private readonly Func<IConnectionManager> connectionManagerFactory;
        private readonly Func<IUpgradeLog> log;
        private readonly IEnumerable<IScriptPreprocessor> scriptPreprocessors;
        private readonly IObjectNameParser objectNameParser;
        private readonly Func<bool> variablesEnabled;

        /// <summary>
        /// Database Schema, should be null if database does not support schemas
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// SQLCommand Timeout in seconds. If not set, the default SQLCommand timeout is not changed.
        /// </summary>
        public int? ExecutionTimeoutSeconds { get; set; }

        protected IObjectNameParser ObjectNameParser
        {
            get { return objectNameParser; }
        }

        protected Func<IConnectionManager> ConnectionManagerFactory
        {
            get { return connectionManagerFactory; }
        }

        /// <summary>
        /// Initializes an instance of the <see cref="DbScriptExecutor"/> class.
        /// </summary>
        /// <param name="connectionManagerFactory"></param>
        /// <param name="log">The logging mechanism.</param>
        /// <param name="schema">The schema that contains the table.</param>
        /// <param name="variablesEnabled">Function that returns <c>true</c> if variables should be replaced, <c>false</c> otherwise.</param>
        /// <param name="scriptPreprocessors">Script Preprocessors in addition to variable substitution</param>
        /// <param name="objectNameParser"></param>
        protected DbScriptExecutor(Func<IConnectionManager> connectionManagerFactory, Func<IUpgradeLog> log, string schema, Func<bool> variablesEnabled,
            IEnumerable<IScriptPreprocessor> scriptPreprocessors, IObjectNameParser objectNameParser)
        {
            Schema = schema;
            this.connectionManagerFactory = connectionManagerFactory;
            this.log = log;
            this.variablesEnabled = variablesEnabled;
            this.scriptPreprocessors = scriptPreprocessors;
            this.objectNameParser = objectNameParser;
        }

        /// <summary>
        /// Executes the specified script against a database at a given connection string.
        /// </summary>
        /// <param name="script">The script.</param>
        public virtual void Execute(SqlScript script)
        {
            Execute(script, null);
        }

        /// <summary>
        /// Executes the specified script against a database at a given connection string.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="variables">Variables to replace in the script</param>
        public virtual void Execute(SqlScript script, IDictionary<string, string> variables)
        {
            if (variables == null)
                variables = new Dictionary<string, string>();
            if (Schema != null && !variables.ContainsKey("schema"))
                variables.Add("schema", objectNameParser.Quote(Schema));

            log().WriteInformation("Executing SQL Server script '{0}'", script.Name);

            var contents = script.Contents;
            if (string.IsNullOrEmpty(Schema))
                contents = new StripSchemaPreprocessor().Process(contents);
            if (variablesEnabled())
                contents = new VariableSubstitutionPreprocessor(variables).Process(contents);
            contents = (scriptPreprocessors ?? new IScriptPreprocessor[0])
                .Aggregate(contents, (current, additionalScriptPreprocessor) => additionalScriptPreprocessor.Process(current));

            var scriptStatements = connectionManagerFactory().SplitScriptIntoCommands(contents);
            var index = -1;
            try
            {
                connectionManagerFactory().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    foreach (var statement in scriptStatements)
                    {
                        index++;
                        using (var command = dbCommandFactory())
                        {
                            command.CommandText = statement;
                            if (ExecutionTimeoutSeconds != null)
                                command.CommandTimeout = ExecutionTimeoutSeconds.Value;
                            command.ExecuteNonQuery();
                        }
                    }
                });
            }
            catch (DbException sqlException)
            {
                log().WriteInformation("DB exception has occured in script: '{0}'", script.Name);
                log().WriteError("Script block number: {0}; Error code {1}; Message: {2}", index, sqlException.ErrorCode, sqlException.Message);
                log().WriteError(sqlException.ToString());
                throw;
            }
            catch (Exception ex)
            {
                log().WriteInformation("Exception has occured in script: '{0}'", script.Name);
                log().WriteError(ex.ToString());
                throw;
            }
        }

        public virtual void VerifySchema()
        {
            if (string.IsNullOrEmpty(Schema)) return;

            ConnectionManagerFactory().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
                {
                    var sqlRunner = new AdHocSqlRunner(dbCommandFactory, ObjectNameParser, Schema, () => true);
                    sqlRunner.ExecuteNonQuery(VerifySchemaSql(Schema));
                });
        }

        protected abstract string VerifySchemaSql(string schema);
    }
}