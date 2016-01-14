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
    public sealed class LocalMessageQueueTests : IClassFixture<RedisServerStarter>
    {
        private readonly LocalMessageQueue _localMessageQueue;

        public LocalMessageQueueTests()
        {
            var dataSerializerMock = new Mock<IDataSerializer>();
            dataSerializerMock.Setup(x => x.ToJson(It.IsAny<AmazonDataMessage>()))
                              .Returns<AmazonDataMessage>(x => x.RawData);
            dataSerializerMock.Setup(x => x.FromJson<AmazonDataMessage>(It.IsAny<string>()))
                              .Returns<string>(x => new AmazonDataMessage {RawData = x}.ToOption());

            var configMock = new Mock<IMessageQueueConfig>();
            configMock.Setup(x => x.QueueUrl).Returns("url");
            configMock.Setup(x => x.DataSerializer).Returns(dataSerializerMock.Object);

            _localMessageQueue = new LocalMessageQueue(configMock.Object);
        }

        [Fact]
        public void Receive_SingleMessage_Ok()
        {
            _localMessageQueue.Send(new AmazonDataMessage {RawData = "msg"});

            List<AmazonDataMessage> messages = _localMessageQueue.Receive();

            Assert.Equal("msg", messages.Single().RawData);
        }

        [Fact]
        public void Receive_TwoMessages_Ok()
        {
            _localMessageQueue.Send(new AmazonDataMessage {RawData = "msg_1"});
            _localMessageQueue.Send(new AmazonDataMessage {RawData = "msg_2"});
            Thread.Sleep(50);

            List<AmazonDataMessage> messages = _localMessageQueue.Receive();

            Assert.Equal(2, messages.Count);
        }
    }
}