using System;
using System.Data;
using System.Data.SQLite;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support.SQLite;
using NSubstitute;
using NUnit.Framework;

namespace DbUp.Tests.SQLite
{
    [TestFixture]
    public class SQLiteTableJournalTests
    {
        [Test]
        public void dbversion_is_zero_when_journal_table_not_exist()
        {
            // Given
            var dbConnection = Substitute.For<IDbConnection>();
            var command = Substitute.For<IDbCommand>();
            dbConnection.CreateCommand().Returns(command);
            var connectionManager = Substitute.For<IConnectionManager>();
            command.ExecuteScalar().Returns(x => { throw new SQLiteException("table not found"); });
            var consoleUpgradeLog = new ConsoleUpgradeLog();
            var journal = new SQLiteTableJournal(() => connectionManager, () => consoleUpgradeLog, "SchemaVersions");

            // When
            var scripts = journal.GetExecutedScripts();

            // Expect
            command.DidNotReceive().ExecuteReader();
            Assert.AreEqual(0, scripts.Length);
        }

        [Test]
        public void creates_a_new_journal_table_when_not_exist()
        {
            // Given
            var dbConnection = Substitute.For<IDbConnection>();
            var connectionManager = new TestConnectionManager(dbConnection, true);
            var command = Substitute.For<IDbCommand>();
            var param = Substitute.For<IDbDataParameter>();
            dbConnection.CreateCommand().Returns(command);
            command.CreateParameter().Returns(param);
            command.ExecuteScalar().Returns(x => { throw new SQLiteException("table not found"); });
            var consoleUpgradeLog = new ConsoleUpgradeLog();
            var journal = new SQLiteTableJournal(() => connectionManager, () => consoleUpgradeLog, "SchemaVersions");

            // When
            journal.StoreExecutedScript(new SqlScript("test", "select 1"));

            // Expect
            Assert.That(command.CommandText, Is.EqualTo("CREATE TABLE TEST_DBUP_SCHEMA.TEST_SCHEMA_VERSION_TABLE (ID VARCHAR2(32) DEFAULT sys_guid() NOT NULL, SCRIPT_NAME VARCHAR2(255) NOT NULL, APPLIED DATE NOT NULL, CONSTRAINT PK_TEST_SCHEMA_VERSION_TABLE PRIMARY KEY (ID) ENABLE VALIDATE)"));
            command.Received().ExecuteNonQuery();
        }
    }
}
