using System;
using System.IO;
using System.Text;
using SystemInterface.IO;
using Moq;
using WindowSelector.Common.Configuration;

namespace WindowSelector.Tests.Utilities
{
    public static class MockExtensions
    {
        public static void PrepareReadableConfigurationProviderMock(this Autofac.Extras.Moq.AutoMock mock, string configFileJsonData = "{}")
        {
            mock.Mock<IFileSystemWatcherFactory>()
                    .Setup(f => f.Create(".", "config.json"))
                    .Returns(Mock.Of<IFileSystemWatcher>());
            mock.Mock<IFile>()
                .Setup(f => f.OpenText("config.json"))
                .Returns(Mock.Of<IStreamReader>(sw => sw.StreamReaderInstance
                    == new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(configFileJsonData)))));
        }

        public static void PrepareWriteableConfigurationProviderMock(this Autofac.Extras.Moq.AutoMock mock, string configFileJsonData = "{}")
        {
            mock.Mock<IFileSystemWatcherFactory>()
                    .Setup(f => f.Create(".", "config.json"))
                    .Returns(Mock.Of<IFileSystemWatcher>());
            mock.Mock<IFile>()
                    .Setup(f => f.CreateText("config.json"))
                    .Returns(Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
        }
        
        public static HandlerWrapper RegisterMockConfigurationUpdateEventListener(this Autofac.Extras.Moq.AutoMock mock)
        {
            HandlerWrapper wrapper = new HandlerWrapper();
            mock.Mock<IConfigurationProvider>()
                .Setup(p => p.AddConfigurationUpdatedHandler(It.IsAny<EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs>>()))
                .Callback((EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs> h) => wrapper.Handler += h);

            return wrapper;
        }
    }

    public class HandlerWrapper
    {
        public EventHandler<ConfigurationProvider.ConfigurationUpdateEventArgs> Handler = delegate(object sender, ConfigurationProvider.ConfigurationUpdateEventArgs args) {  };



    }
}
