using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Oracle;
using NSubstitute;
using NUnit.Framework;

namespace DbUp.Tests.Oracle
{
    [TestFixture]
    public class OracleScriptExecutorTests
    {
        private MockConnectionManager connectionManager;
        private IUpgradeLog logger;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<IUpgradeLog>();
            connectionManager = new MockConnectionManager("");
            connectionManager.UpgradeStarting(logger);
        }

        [Test]
        public void uses_variable_substitute_preprocessor_when_running_scripts()
        {
            var sut = new OracleScriptExecutor(() => connectionManager, () => logger, null, () => true, null);
            var variables = new Dictionary<string, string> { { "columnDefault", "NULL" } };

            // Act
            sut.Execute(new SqlScript("Test", "CREATE TABLE1(COL1 NUMBER DEFAULT $columnDefault$)"), variables);

            // Assert
            Assert.That(connectionManager.Commands.First().CommandText, Is.EqualTo("CREATE TABLE1(COL1 NUMBER DEFAULT NULL)"));
            connectionManager.Commands.First().Received().ExecuteNonQuery();
        }

        [Test]
        public void does_not_use_variable_substitute_preprocessor_when_setting_false()
        {
            var sut = new OracleScriptExecutor(() => connectionManager, () => logger, null, () => false, null);
            var variables = new Dictionary<string, string> { { "columnDefault", "NULL" } };
            
            // Act
            sut.Execute(new SqlScript("Test", "CREATE TABLE1(COL1 NUMBER DEFAULT $columnDefault$)"), variables);

            // Assert
            Assert.That(connectionManager.Commands.First().CommandText, Is.EqualTo("CREATE TABLE1(COL1 NUMBER DEFAULT $columnDefault$)"));
            connectionManager.Commands.First().Received().ExecuteNonQuery();
        }

        [Test]
        public void uses_variable_substitutes()
        {
            var sut = new OracleScriptExecutor(() => connectionManager, () => logger, null, () => true, null);
            var variables = new Dictionary<string, string> { { "variable1", "SCHEMA" } };

            // Act
            sut.Execute(new SqlScript("Test", "create $variable1$.Table"), variables);

            // Assert
            Assert.That(connectionManager.Commands.First().CommandText, Is.EqualTo("create SCHEMA.Table"));
            connectionManager.Commands.First().Received().ExecuteNonQuery();
        }

        [Test]
        public void execute_splits_scripts_into_batches_seperated_by_a_forward_slash_on_own_line()
        {
            const string script = @"CREATE TABLE BLAH
/
CREATE TABLE FOO
/
CREATE TABLE BAR";

            var sut = new OracleScriptExecutor(() => connectionManager, () => logger, null, () => true, null);

            // Act
            sut.Execute(new SqlScript("Test", script));

            // Assert
            connectionManager.Commands.First().Received(3).ExecuteNonQuery();
        }

        [Test]
        public void execute_ignores_empty_batches()
        {
            const string script = @"CREATE TABLE BLAH
/
CREATE TABLE FOO
/

/
CREATE TABLE BAR";

            var sut = new OracleScriptExecutor(() => connectionManager, () => logger, null, () => true, null);

            // Act
            sut.Execute(new SqlScript("Test", script));

            // Assert
            connectionManager.Commands.First().Received(3).ExecuteNonQuery();
        }
    }
}