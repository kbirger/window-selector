using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Threading;
using Moq;
using WindowSelector.Win32;

namespace WindowSelector.Tests
{
    [TestFixture]
    public class IconRetrieverFixture
    {
        [Test]
        public void Test_LoadIcon_loads_tempicon()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                IntPtr hWnd = new IntPtr(0xd34db33f);
                var tempIcon = ResourceUtil.GetIcon();
                mock.Provide(Dispatcher.CurrentDispatcher);
                mock.Mock<IWin32ApiWrapper>().Setup(api => api.GetAppClassIcon(hWnd)).Returns(tempIcon);
                IconRetriever retriever = mock.Create<IconRetriever>();
                // Act
                retriever.LoadIcon(hWnd);
                // Assert
                Assert.That(retriever.Icon, Is.EqualTo(tempIcon));
                Assert.That(retriever.Image.Width, Is.EqualTo(16));
            }
        }

        [Test]
        public void Test_LoadIcon_loads_finalicon()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                IntPtr hWnd = new IntPtr(0xd34db33f);
                var tempIcon = ResourceUtil.GetIcon();
                mock.Provide(Dispatcher.CurrentDispatcher);
                mock.Mock<IWin32ApiWrapper>().Setup(api => api.GetAppClassIcon(hWnd)).Returns((Icon)null);
                mock.Mock<IWin32ApiWrapper>()
                    .Setup(
                        api =>
                            api.SendMessageAsync(hWnd, It.IsAny<uint>(), It.IsAny<int>(),
                                It.IsAny<Win32Api.SendMessageDelegate>()))
                    .Callback(new Action<IntPtr, uint, int, Win32Api.SendMessageDelegate>((handle, msg, wParam, callback) =>
                    {
                        callback(handle, msg, UIntPtr.Zero, tempIcon.Handle);
                    }));
                IconRetriever retriever = mock.Create<IconRetriever>();
                // Act
                retriever.LoadIcon(hWnd);
                // Assert
                Assert.That(retriever.Icon.Width, Is.EqualTo(tempIcon.Width));
                Assert.That(retriever.Image.Width, Is.EqualTo(16));
            }
        }

        [Test]
        public void Test_LoadIcon_loads_finalicon_after_first_fail()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                IntPtr hWnd = new IntPtr(0xd34db33f);
                var tempIcon = ResourceUtil.GetIcon();
                mock.Provide(Dispatcher.CurrentDispatcher);
                mock.Mock<IWin32ApiWrapper>().Setup(api => api.GetAppClassIcon(hWnd)).Returns((Icon)null);
                mock.Mock<IWin32ApiWrapper>()
                    .Setup(
                        api =>
                            api.SendMessageAsync(hWnd, It.IsAny<uint>(), 1,
                                It.IsAny<Win32Api.SendMessageDelegate>()))
                    .Callback(new Action<IntPtr, uint, int, Win32Api.SendMessageDelegate>((handle, msg, wParam, callback) =>
                    {
                        callback(handle, msg, UIntPtr.Zero, IntPtr.Zero);
                    }));

                mock.Mock<IWin32ApiWrapper>()
                    .Setup(
                        api =>
                            api.SendMessageAsync(hWnd, It.IsAny<uint>(), 2,
                                It.IsAny<Win32Api.SendMessageDelegate>()))
                    .Callback(new Action<IntPtr, uint, int, Win32Api.SendMessageDelegate>((handle, msg, wParam, callback) =>
                    {
                        callback(handle, msg, UIntPtr.Zero, tempIcon.Handle);
                    }));
                IconRetriever retriever = mock.Create<IconRetriever>();
                // Act
                retriever.LoadIcon(hWnd);
                // Assert
                Assert.That(retriever.Icon.Width, Is.EqualTo(tempIcon.Width));
                Assert.That(retriever.Image.Width, Is.EqualTo(16));
            }
        }

        [Test]
        public void Test_LoadIcon_loads_no_final_icon()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                IntPtr hWnd = new IntPtr(0xd34db33f);
                var tempIcon = ResourceUtil.GetIcon();
                mock.Provide(Dispatcher.CurrentDispatcher);
                mock.Mock<IWin32ApiWrapper>().Setup(api => api.GetAppClassIcon(hWnd)).Returns(tempIcon);
                mock.Mock<IWin32ApiWrapper>()
                    .Setup(
                        api =>
                            api.SendMessageAsync(hWnd, It.IsAny<uint>(), It.IsAny<int>(),
                                It.IsAny<Win32Api.SendMessageDelegate>()))
                    .Callback(new Action<IntPtr, uint, int, Win32Api.SendMessageDelegate>((handle, msg, wParam, callback) =>
                    {
                        callback(handle, msg, UIntPtr.Zero, IntPtr.Zero);
                    }));

                
                IconRetriever retriever = mock.Create<IconRetriever>();
                // Act
                retriever.LoadIcon(hWnd);
                // Assert
                // verify that we still have the original icon
                Assert.That(retriever.Icon, Is.EqualTo(tempIcon));
            }
        }

        [Test]
        public void Test_LoadIcon_calls_PropertyChanged()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                IntPtr hWnd = new IntPtr(0xd34db33f);
                var tempIcon = ResourceUtil.GetIcon();
                mock.Provide(Dispatcher.CurrentDispatcher);
                mock.Mock<IWin32ApiWrapper>().Setup(api => api.GetAppClassIcon(hWnd)).Returns(tempIcon);
                IconRetriever retriever = mock.Create<IconRetriever>();
                bool called = false;
                retriever.PropertyChanged += (sender, args) => called = true;
                // Act
                retriever.LoadIcon(hWnd);
                // Assert
                Assert.That(called, Is.True);
            }
        }
    }
}