using System;
using System.Windows.Threading;
using Moq;
using NUnit.Framework;
using WindowSelector.Common.Configuration;
using WindowSelector.Common.Interfaces;
using WindowSelector.Plugins.Win32.Models;
using WindowSelector.Plugins.Win32.ViewModels;
using WindowSelector.Tests.Utilities;

namespace WindowSelector.Tests.ViewModels
{
    [TestFixture]
    public class NativeWindowResultFixture
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
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID= 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"
            });
            var result = mock.Create<NativeWindowResult>();
            // Act
            result.Blacklist();
            // Assert
            Assert.That(_config.Blacklist, Contains.Item("chromeWindow"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_whitelist()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID= 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"
            });
            var result = mock.Create<NativeWindowResult>();
            // Act
            result.Whitelist();
            // Assert
            Assert.That(_config.Whitelist, Contains.Item("chromeWindow"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_unblacklist()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID= 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"
                
            });
            _config.Blacklist.Add("chromeWindow");
            var result = mock.Create<NativeWindowResult>();
            // Act
            result.UnBlacklist();
            // Assert
            Assert.That(_config.Blacklist, Has.No.Member("chromeWindow"));
            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_unwhitelist()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            });
            _config.Whitelist.Add("chromeWindow");
            var result = mock.Create<NativeWindowResult>();
            // Act
            result.UnWhitelist();
            // Assert
            Assert.That(_config.Whitelist, Has.No.Member("chromeWindow"));

            mock.Mock<IConfigurationProvider>().Verify(m => m.SaveConfiguration(_config));
        }

        [Test]
        public void Test_select_calls_api()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            });
            var result = mock.Create<NativeWindowResult>();
            
            // Act
            result.Select(true);
            mock.Mock<IWindowManager>().Verify(w => w.ShowWindow(new IntPtr(1)));
            mock.Mock<IWindowManager>().Verify(w => w.CenterWindow(new IntPtr(1)));
        }

        [Test]
        public void Test_select_calls_api_no_center()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            });
            var result = mock.Create<NativeWindowResult>();

            // Act
            result.Select(false);
            mock.Mock<IWindowManager>().Verify(w => w.ShowWindow(new IntPtr(1)));
            mock.Mock<IWindowManager>().Verify(w => w.CenterWindow(new IntPtr(1)), Times.Never);
        }

        [Test]
        public void Test_minimize_calls_api()
        {
            // Arrange
            mock.Provide(new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            });

            var result = mock.Create<NativeWindowResult>();
            
            // Act
            result.Minimize();

            // Assert
            mock.Mock<IWindowManager>().Verify(w => w.MinimizeWindow(new IntPtr(1)));
        }

        [Test]
        public void Test_value_set_correctly()
        {
            // Arrange
            var window = new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            };
            mock.Provide(window);

            // Act
            var result = mock.Create<NativeWindowResult>();

            // Assert
            Assert.That(result.Value, Is.EqualTo(window));
        }

        [Test]
        [TestCase("Blacklist", true)]
        [TestCase("ActivationKey", false)]
        public void Test_ConfigurationUpdated_reloads_config(string propertyName, bool testResult)
        {
            // Arrange
            var window = new NativeWindowInfo
            {
                Title = "My tab",
                PID = 123,
                Icon = null,
                hWnd = new IntPtr(1),
                
                ProcessName = "chrome",
                Wndclass = "chromeWindow"

            };
            mock.Provide(window);
            var eventSource = mock.RegisterMockConfigurationUpdateEventListener();

            // Act
            var result = mock.Create<NativeWindowResult>();
            _config.Blacklist.Add("chromeWindow");

            eventSource.Handler.Invoke(null, new ConfigurationProvider.ConfigurationUpdateEventArgs(new string[] { propertyName }));
            // Assert
            Assert.That(result.IsBlackListed, Is.EqualTo(testResult));
        }

    }
}
