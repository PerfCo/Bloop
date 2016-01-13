using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.SimpleQueue;
using Amazon.SimpleQueue.Messages;
using Core.Serializers;
using Moq;
using Nelibur.Sword.Extensions;
using UnitTests.ClassFixtures;
using Xunit;

namespace UnitTests
{
    public class LocalMessageQueueTests : IClassFixture<RedisServerStarter>
    {
        private readonly LocalMessageQueue _localMessageQueue;

        public LocalMessageQueueTests()
        {
            var dataSerializerMock = new Mock<IDataSerializer>();
            dataSerializerMock.Setup(x => x.ToJson(It.IsAny<FakeAmazonDataMessage>()))
                .Returns<FakeAmazonDataMessage>(x => x.Data);
            dataSerializerMock.Setup(x => x.FromJson<AmazonDataMessage>(It.IsAny<string>()))
                .Returns<string>(x => ((AmazonDataMessage)new FakeAmazonDataMessage {RawData = x}).ToOption());

            var configMock = new Mock<IMessageQueueConfig>();
            configMock.Setup(x => x.QueueUrl).Returns("url");
            configMock.Setup(x => x.DataSerializer).Returns(dataSerializerMock.Object);

            _localMessageQueue = new LocalMessageQueue(configMock.Object);
        }

        [Fact]
        public void Receive_SingleMessage_Ok()
        {
            _localMessageQueue.Send(new FakeAmazonDataMessage {RawData = "msg"});

            List<FakeAmazonDataMessage> messages = _localMessageQueue.Receive<FakeAmazonDataMessage>();

            Assert.Equal("msg", messages.Single().Data);
        }

        [Fact]
        public void Receive_TwoMessages_Ok()
        {
            _localMessageQueue.Send(new FakeAmazonDataMessage { RawData = "msg_1" });
            _localMessageQueue.Send(new FakeAmazonDataMessage { RawData = "msg_2" });
            Thread.Sleep(50);

            List<FakeAmazonDataMessage> messages = _localMessageQueue.Receive<FakeAmazonDataMessage>();

            Assert.Equal(2, messages.Count);
        }

        private class FakeAmazonDataMessage : AmazonDataMessage
        {
            public string Data => RawData;
        }
    }
}