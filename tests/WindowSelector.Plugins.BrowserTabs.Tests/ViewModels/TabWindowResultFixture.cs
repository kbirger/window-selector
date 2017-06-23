using System;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Moq;
using NLog;
using NUnit.Framework;
using WindowSelector.Common.Configuration;
using WindowSelector.Plugins.BrowserTabs.Models;
using WindowSelector.Plugins.BrowserTabs.ViewModels;
using WindowSelector.Tests.Utilities;

namespace WindowSelector.Plugins.BrowserTabs.Tests.ViewModels
{
    [TestFixture]
    public class TabWindowResultFixture
    {
        private Autofac.Extras.Moq.AutoMock mock;
        private ConfigurationRoot _config;

        [SetUp]
        public void Setup()
        {
            _config = new ConfigurationRoot();
            mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            mock.Provide(Dispatcher.CurrentDispatcher);
            mock.Mock<IConfigurationProvider>().Setup(m => m.GetConfiguration())
                .Returns(_config);
        }

        [TearDown]
        public void Teardown()
        {
            mock.Dispose();
            mock = null;
        }

        [Test]
        public void Test_blacklist()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            var result = mock.Create<TabWindowResult>();
            // Act
            result.Blacklist();
            // Assert
            Assert.That(_config.Blacklist, Contains.Item("http://www.google.com/"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_whitelist()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            var result = mock.Create<TabWindowResult>();
            // Act
            result.Whitelist();
            // Assert
            Assert.That(_config.Whitelist, Contains.Item("http://www.google.com/"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_unblacklist()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            _config.Blacklist.Add("http://www.google.com/");
            var result = mock.Create<TabWindowResult>();
            // Act
            result.UnBlacklist();
            // Assert
            Assert.That(_config.Blacklist, Has.No.Member("http://www.google.com/"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_unwhitelist()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            var result = mock.Create<TabWindowResult>();
            _config.Whitelist.Add("chromeWindow");

            // Act
            result.UnWhitelist();
            // Assert
            Assert.That(_config.Whitelist, Has.No.Member("http://www.google.com/"));

            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_select_calls_hub()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            mock.Mock<IConnectionManager>()
                .Setup(m => m.GetHubContext<ChromeTabsHub, IChromeExtension>())
                .Returns(mock.Mock<IHubContext<IChromeExtension>>().Object);
            var result = mock.Create<TabWindowResult>();
            var clientsMock = new Mock<IChromeExtension>();
            mock.Mock<IHubContext<IChromeExtension>>().SetupGet(m => m.Clients.All).Returns(clientsMock.Object);
            // Act
            result.Select(true);
            clientsMock.Verify(c => c.SetTab(1, 123, null, true));
        }

        [Test]
        public void Test_minimize_calls_hub()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            mock.Mock<IConnectionManager>()
                .Setup(m => m.GetHubContext<ChromeTabsHub, IChromeExtension>())
                .Returns(mock.Mock<IHubContext<IChromeExtension>>().Object);
            var result = mock.Create<TabWindowResult>();
            var clientsMock = new Mock<IChromeExtension>();
            mock.Mock<IHubContext<IChromeExtension>>().SetupGet(m => m.Clients.All).Returns(clientsMock.Object);
            
            // Act
            result.Minimize();

            // Assert
            clientsMock.Verify(c => c.SetTab(1, 123, It.Is<WindowUpdateInfo>(w => w.State == WindowState.minimized), false));
        }

        [Test]
        public void Test_close_calls_hub()
        {
            // Arrange
            mock.Provide(new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "http://www.google.com/favico.ico",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            });
            mock.Mock<IConnectionManager>()
                .Setup(m => m.GetHubContext<ChromeTabsHub, IChromeExtension>())
                .Returns(mock.Mock<IHubContext<IChromeExtension>>().Object);
            mock.Mock<IHubContext<IChromeExtension>>().SetupGet(m => m.Clients.All).Returns(mock.Mock<IChromeExtension>().Object);

            var result = mock.Create<TabWindowResult>();

            // Act
            result.Close();

            // Assert
            mock.Mock<IChromeExtension>().Verify(c => c.CloseTabs(123));
        }

        [Test]
        public void Test_value_set_correctly()
        {
            // Arrange
            var tab = new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = null,
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            };
            mock.Provide(tab);

            // Act
            var result = mock.Create<TabWindowResult>();

            // Assert
            Assert.That(result.Value, Is.EqualTo(tab));
        }

        [Test]
        [TestCase("Blacklist", true)]
        [TestCase("ActivationKey", false)]
        public void Test_ConfigurationUpdated_reloads_config(string propertyName, bool testResult)
        {
            // Arrange
            var tab = new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = null,
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            };
            mock.Provide(tab);
            var eventSource = mock.RegisterMockConfigurationUpdateEventListener();

            // Act
            var result = mock.Create<TabWindowResult>();
            _config.Blacklist.Add("http://www.google.com/");
            
            eventSource.Handler.Invoke(null, new ConfigurationProvider.ConfigurationUpdateEventArgs(new string[] { propertyName }));

            // Assert
            Assert.That(result.IsBlackListed, Is.EqualTo(testResult));
        }

        [Test]
        public void Test_ToString()
        {
            // Arrange
            var tab = new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = null,
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            };
            mock.Provide(tab);

            // Act
            var result = mock.Create<TabWindowResult>();

            // Assert
            Assert.That(result.ToString(), Is.EqualTo("[Tab Window] Title: My tab; Url: http://www.google.com/"));
        }

        [Test]
        public void Test_invalid_uri_scheme_sets_null_and_logs()
        {
            // Arrange
            var tab = new TabInfo
            {
                Title = "My tab",
                Id = 123,
                FavIconUrl = "magnet://www.google.com/",
                Thumb = null,
                Url = "http://www.google.com/",
                WindowId = 1
            };
            mock.Provide(tab);
            // Act
            var result = mock.Create<TabWindowResult>();
            // Assert
            Assert.That(result.Icon, Is.Null);
            mock.Mock<ILogger>().Verify(l => l.Warn(It.IsAny<NotSupportedException>()));
        }

    }
}
