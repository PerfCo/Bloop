using System;
using Amazon.SimpleQueue;
using Amazon.SQS;
using Core.Serializers;
using Moq;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using Xunit;

namespace UnitTests
{
    public sealed class ForTest
    {
        [Fact]
        public void Test()
        {
        }

        [Fact]
        public void LocalMessageQueueTest()
        {
            throw new NotImplementedException();
//            var dataSerializerMock = new Mock<IDataSerializer>();
//            dataSerializerMock.Setup(x => x.ToJson(It.IsAny<string>())).Returns<object>(x => x.ToString());
//            dataSerializerMock.Setup(x => x.FromJson<string>(It.IsAny<string>())).Returns<string>(x => x.ToOption());
//            var configMock = new Mock<IMessageQueueConfig>();
//            configMock.Setup(x => x.QueueUrl).Returns("url");
//            configMock.Setup(x => x.DataSerializer).Returns(dataSerializerMock.Object);
//            var localMessageQueue = new LocalMessageQueue(configMock.Object);
//
//            var message = localMessageQueue.Receive<string>();
//
//            //use redis-cli.exe to check if the test is unblocked
//            //type a command: publish url msg
//            Assert.Equal(message, "msg");
        }
    }
}