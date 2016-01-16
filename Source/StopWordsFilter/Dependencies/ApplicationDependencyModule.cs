using Amazon.SimpleQueue;
using Core.Serializers;
using Nelibur.Sword.Threading.ThreadPools;
using Ninject;
using Ninject.Modules;
using StopWordsFilter.Processors;
using StopWordsFilter.Properties;

namespace StopWordsFilter.Dependencies
{
    public sealed class ApplicationDependencyModule : NinjectModule
    {
        private readonly Settings _settings = Settings.Default;

        public override void Load()
        {
            Bind<IDataSerializer>().To<DataSerializer>().InSingletonScope();
            RegisterMessageQueue();
        }

        private void RegisterMessageQueue()
        {
            ITinyThreadPool threadPool = TinyThreadPool.Create(x =>
            {
                x.MaxThreads = 3;
                x.MultiThreadingCapacity = MultiThreadingCapacity.PerProcessor;
            });

            IMessageQueue inputQueue = QueueConfiguration.Create()
                                                         .Configure(config =>
                                                         {
                                                             config.DataSerializer = KernelInstance.Get<IDataSerializer>();
                                                             config.QueueUrl = _settings.InputTweetQueueUrl;
                                                         }).CreateLocalQueue();

            IMessageQueue outputQueue = QueueConfiguration.Create()
                                                          .Configure(config =>
                                                          {
                                                              config.DataSerializer = KernelInstance.Get<IDataSerializer>();
                                                              config.QueueUrl = _settings.OutputTweetQueueUrl;
                                                          }).CreateLocalQueue();

            Bind<StopWordsProcessor>().ToConstructor(x => new StopWordsProcessor(inputQueue, outputQueue, threadPool, x.Inject<IDataSerializer>()))
                                      .InSingletonScope();
        }
    }
}