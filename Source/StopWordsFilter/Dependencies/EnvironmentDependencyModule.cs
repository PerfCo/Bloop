using Amazon.SimpleQueue;
using Core.Serializers;
using Ninject;
using Ninject.Modules;
using StopWordsFilter.Properties;

namespace StopWordsFilter.Dependencies
{
    public sealed class EnvironmentDependencyModule : NinjectModule
    {
        private readonly Settings _settings = Settings.Default;

        public override void Load()
        {
            Bind<IDataSerializer>().To<DataSerializer>().InSingletonScope();
            RegisterMessageQueue();
        }

        private void RegisterMessageQueue()
        {
            IMessageQueue inputQueue = QueueConfiguration.Create()
                                                         .Configure(config =>
                                                         {
                                                             config.DataSerializer = KernelInstance.Get<IDataSerializer>();
                                                             config.QueueUrl = _settings.InputTweetQueueUrl;
                                                         }).CreateLocalQueue();
        }
    }
}