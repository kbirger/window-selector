using System;
using NUnit.Framework;
using Moq;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Castle.Core.Internal;
using WindowSelector.Common.Configuration;
using WindowSelector.Plugins.Win32;
using WindowSelector.Tests._Utils;
using WindowSelector.Win32;

namespace WindowSelector.Tests
{
    [TestFixture]
    public class WindowManagerFixture
    {
        private Autofac.Extras.Moq.AutoMock mock;

        [SetUp]
        public void Setup()
        {
            // todo: get rid of external dependency. Maybe wrap Process
            mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var curProc = Process.GetCurrentProcess();
            var curThreadId = curProc.Threads[0].Id;

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.EnumThreadWindows((uint) curThreadId, It.IsAny<Win32Api.EnumThreadDelegate>()))
                .Callback<uint, Win32Api.EnumThreadDelegate>((hWnd, callback) =>
                {
                    new int[] {1, 2, 3, 4, 5}.ForEach(i => callback(new IntPtr(i), IntPtr.Zero));
                    //callback(hwnd, IntPtr.Zero);
                });

            // Add window "foo" as hwnd 1 to current thread
            IntPtr hwnd = new IntPtr(1);
            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowText(hwnd))
                .Returns("foo foo foo");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetClassName(hwnd))
                .Returns("fooClass");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowInfo(hwnd))
                .Returns(new WINDOWINFO()
                {
                    dwStyle = WindowStyles.WS_VISIBLE | WindowStyles.WS_CAPTION
                });

            // Add window "blacklisted" as hwnd 2 to current thread
            hwnd = new IntPtr(2);
            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowText(hwnd))
                .Returns("blacklisted");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetClassName(hwnd))
                .Returns("blacklistedClass");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowInfo(hwnd))
                .Returns(new WINDOWINFO()
                {
                    dwStyle = WindowStyles.WS_VISIBLE | WindowStyles.WS_CAPTION
                });

            // Add window "nonMatch" as hwnd 3 to current thread
            hwnd = new IntPtr(3);
            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowText(hwnd))
                .Returns("nonMatch");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetClassName(hwnd))
                .Returns("nonmatchClass");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowInfo(hwnd))
                .Returns(new WINDOWINFO()
                {
                    dwStyle = WindowStyles.WS_VISIBLE | WindowStyles.WS_CAPTION
                });

            // Add window "whiteListed" as hwnd 4 to current thread
            hwnd = new IntPtr(4);

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowText(hwnd))
                .Returns("whitelisted");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetClassName(hwnd))
                .Returns("whitelistedClass");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowInfo(hwnd))
                .Returns(new WINDOWINFO()
                {
                    dwStyle = WindowStyles.WS_VISIBLE | WindowStyles.WS_CAPTION
                });

            // Add window "ninja" as hwnd 5 to current thread
            hwnd = new IntPtr(5);

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowText(hwnd))
                .Returns("ninja");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetClassName(hwnd))
                .Returns("ninjaClass");

            mock.Mock<IWin32ApiWrapper>()
                .Setup(m => m.GetWindowInfo(hwnd))
                .Returns(new WINDOWINFO()
                {
                    dwStyle = WindowStyles.WS_DISABLED
                });

            // setup config
            var config = new ConfigurationRoot()
            {
                Whitelist = {
                    "whitelistedClass"
                },
                Blacklist = 
                {
                    "blacklistedClass"
                }
            };
            mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);

            mock.Provide(Dispatcher.CurrentDispatcher);

        }

        [TearDown]
        public void Teardown()
        {
            mock.Dispose();
            mock = null;
        }

        [Test]
        public void Test_FindProcesses_match()
        {
            var windowManager = mock.Create<WindowManager>();
            // Act
            var results = windowManager.FindWindowsWithProcess("foo", true).ToArray();

            // Assert
            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results.First().Title, Is.EqualTo("foo foo foo"));
        }

        [Test]
        public void Test_FindProcesses_ignore_hidden()
        {
            var windowManager = mock.Create<WindowManager>();
            // Act
            var results = windowManager.FindWindowsWithProcess("ninja", false).ToArray();

            // Assert
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [Test]
        public void Test_FindProcesses_shows_hidden_on_showall()
        {
            var windowManager = mock.Create<WindowManager>();
            // Act
            var results = windowManager.FindWindowsWithProcess("ninja", true).ToArray();

            // Assert
            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results.First().Title, Is.EqualTo("ninja"));
        }

        

        [Test]
        public void Test_FindProcesses_blacklist_ignored_when_showall_false()
        {
            // Arrange 
            var windowManager = mock.Create<WindowManager>();

            // Act
            var results = windowManager.FindWindowsWithProcess("black", false).ToArray();

            // Assert
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [Test]
        public void Test_FindProcesses_returns_empty_on_empty_input()
        {
            // Arrange 
            var windowManager = mock.Create<WindowManager>();

            // Act
            var results = windowManager.FindWindowsWithProcess("", false).ToArray();

            // Assert
            Assert.That(results.Length, Is.EqualTo(0));
        }

        [TestCase(Win32Api.WindowShowStyle.Hide, Win32Api.WindowShowStyle.Restore)]
        [TestCase(Win32Api.WindowShowStyle.Minimize, Win32Api.WindowShowStyle.Restore)]
        [TestCase(Win32Api.WindowShowStyle.ShowMinimized, Win32Api.WindowShowStyle.Restore)]
        [TestCase(Win32Api.WindowShowStyle.ShowNormal, null)]
        [TestCase(Win32Api.WindowShowStyle.Maximize, null)]
        [TestCase(Win32Api.WindowShowStyle.Show, Win32Api.WindowShowStyle.ShowNormal)]

        public void Test_ShowWindow_calls_api(Win32Api.WindowShowStyle currentMode, Win32Api.WindowShowStyle? command)
        {
            // Arrange
            var hWnd = new IntPtr(999);
            mock.Mock<IWin32ApiWrapper>().Setup(a => a.GetWindowPlacement(hWnd))
                .Returns(new Win32Api.WINDOWPLACEMENT()
            {
              showCmd  = currentMode
            });
            var windowManager = mock.Create<WindowManager>();

            // Act
           windowManager.ShowWindow(hWnd);

            // Assert
            if (command != null)
            {
                mock.Mock<IWin32ApiWrapper>().Verify(a => a.ShowWindow(hWnd, command.Value));
            }
            else
            {
                mock.Mock<IWin32ApiWrapper>().Verify(a => a.ShowWindow(hWnd, It.IsAny<Win32Api.WindowShowStyle>()), Times.Never);

            }
        }

        [Test]
        public void Test_MinimizeWindow_calls_api()
        {
            // Arrange
            var hWnd = new IntPtr(999);

            var windowManager = mock.Create<WindowManager>();

            // Act
            windowManager.MinimizeWindow(hWnd);

            // Assert
            mock.Mock<IWin32ApiWrapper>().Verify(a => a.ShowWindow(hWnd, Win32Api.WindowShowStyle.ShowMinNoActivate));
        }

        [Test]
        public void Test_CenterWindow_centers_and_calls()
        {
            // Arrange
            var hWnd = new IntPtr(999);
            // fullscreen window with edges touching screen
            mock.Mock<IWin32ApiWrapper>().Setup(a => a.GetWindowRect(hWnd))
                .Returns(new RECT()
                {
                    Left = 0,
                    Top = 0,
                    Right = (int)SystemParameters.PrimaryScreenWidth,
                    Bottom = (int)SystemParameters.PrimaryScreenHeight
                    
                });
            var windowManager = mock.Create<WindowManager>();

            // Act
            windowManager.CenterWindow(hWnd);

            // Assert
            mock.Mock<IWin32ApiWrapper>().Verify(a => a.MoveWindow(hWnd, 0, 0));
        }

        [Test]
        public void Test_Responds_to_configuration_changes()
        {
            // Arrange
            // not used explicitly, but should listen to events
            var eventSource = mock.RegisterMockConfigurationUpdateEventListener();

            var windowManager = mock.Create<WindowManager>();
            // Act
            mock.Mock<IConfigurationProvider>().ResetCalls();

            eventSource.Handler.Invoke(null,
                new ConfigurationProvider.ConfigurationUpdateEventArgs(new string[] {"Blacklist"}));
            // Assert
            mock.Mock<IConfigurationProvider>().Verify(m => m.GetConfiguration(), Times.Exactly(1));
        }

        [Test]
        public void Test_ignores_irrelevant_changes()
        {
            // Arrange
            // not used explicitly, but should listen to events
            var windowManager = mock.Create<WindowManager>();

            // Act
            mock.Mock<IConfigurationProvider>().ResetCalls();

            mock.Mock<IConfigurationProvider>().Raise(p => p.ConfigurationUpdated += null, new ConfigurationProvider.ConfigurationUpdateEventArgs(new string[] { "ActivationKey" }));

            // Assert
            mock.Mock<IConfigurationProvider>().Verify(m => m.GetConfiguration(), Times.Never);
        }
    }
}
