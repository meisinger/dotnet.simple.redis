using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Redis.Tests
{
    [TestClass]
    public class SanityCheckTest
    {
        private string redis_connection = string.Empty;

        [TestInitialize]
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            redis_connection = config.GetConnectionString("RedisServer");
        }

        [TestMethod]
        public void Can_Connect()
        {
            var factory = new RedisConnectionFactory(redis_connection);
            using var connection = factory.Open();

            var command = RedisCommand.Create(RedisCommands.PING);
            var response = command.Execute(connection);
            var message = response[0].AsString();
            Assert.IsTrue(connection.IsOpen);
            Assert.IsTrue(connection.IsActive);
            Assert.IsFalse(connection.IsPassThrough);
        }

        [TestMethod]
        public void Can_Execute_PONG_Command()
        {
            var factory = new RedisConnectionFactory(redis_connection);
            using var connection = factory.Open();

            var command = RedisCommand.Create(RedisCommands.PING);
            var response = command.Execute(connection);
            var message = response[0].AsString();
            Assert.AreEqual("PONG", message);
        }
    }
}
