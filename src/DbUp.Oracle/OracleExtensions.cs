using System;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Oracle;
using DbUp.Support;
using DbUp.Support.SqlServer;

/// <summary>
/// Configuration extension methods for Oracle.
/// </summary>
/// <remarks>
/// NOTE: DO NOT MOVE THIS TO A NAMESPACE
/// Since the class just contains extension methods, we leave it in the root so that it is always discovered
/// and people don't have to manually add using statements.
/// </remarks>

// ReSharper disable CheckNamespace
public static class OracleExtensions
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Creates an upgrader for Oracle databases
    /// </summary>
    /// <param name="supported">Fluent helper type</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>
    /// A builder for an Oracle database upgrader
    /// </returns>
    public static UpgradeEngineBuilder OracleDatabase(this SupportedDatabases supported, string connectionString)
    {
        var builder = new UpgradeEngineBuilder();
        builder.Configure(c => c.ConnectionManager = new OracleConnectionManager(connectionString));
        builder.Configure(c => c.Journal = new OracleTableJournal(() => c.ConnectionManager, () => c.Log));
        builder.Configure(c => c.ScriptExecutor = new OracleScriptExecutor(() => c.ConnectionManager, () => c.Log, null, () => c.VariablesEnabled, c.ScriptPreprocessors));

        return builder;
    }
}