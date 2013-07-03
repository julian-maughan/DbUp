using System;
using System.Collections.Generic;
using System.Data;
using DbUp.Oracle;
using NSubstitute;

namespace DbUp.Tests.Oracle
{
    public class MockConnectionManager : OracleConnectionManager
    {
        private readonly IList<IDbCommand> commands = new List<IDbCommand>();

        public MockConnectionManager(string connectionString)
            : base(connectionString)
        {
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            var connection = Substitute.For<IDbConnection>();
            var command = Substitute.For<IDbCommand>();
            commands.Add(command);
            connection.CreateCommand().Returns(command);
            return connection;
        }

        public IList<IDbCommand> Commands
        {
            get { return commands; }
        }
    }
}