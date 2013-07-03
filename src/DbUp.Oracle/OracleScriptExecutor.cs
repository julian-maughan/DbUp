using System;
using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Oracle
{
    /// <summary>
    /// An implementation of the IScriptExecutor interface that executes against an Oracle database.
    /// </summary>
    public class OracleScriptExecutor : DbScriptExecutor
    {
        public OracleScriptExecutor(Func<IConnectionManager> connectionManagerFactory, Func<IUpgradeLog> log, string schema, Func<bool> variablesEnabled, IEnumerable<IScriptPreprocessor> scriptPreprocessors)
            : base(connectionManagerFactory, log, schema, variablesEnabled, scriptPreprocessors, new OracleObjectNameParser())
        {
        }

        public override void VerifySchema()
        {
        }

        protected override string VerifySchemaSql(string schema)
        {
            throw new NotImplementedException("Implementation not required, as VerifySchema does nothing");
        }
    }
}