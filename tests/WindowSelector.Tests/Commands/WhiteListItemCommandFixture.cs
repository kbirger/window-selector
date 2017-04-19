using System;
using System.Windows.Threading;
using Autofac;
using NUnit.Framework;
using Moq;
using WindowSelector.Commands;
using WindowSelector.Common.ViewModels;
using WindowSelector.Plugins.Win32.ViewModels;

namespace WindowSelector.Tests.Commands
{
    [TestFixture]
    public class WhiteListItemCommandFixture
    {
        [Test]
        [TestCase(null)]
        [TestCase("foo")]
        public void Test_can_execute_always_true(object parameter)
        {
            // Arrange
            var cmd = new WhiteListItemCommand();

            // Assert
            Assert.That(cmd.CanExecute(parameter), Is.True);

        }

        [Test]
        public void Test_can_execute_always_true_with_windowresult()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                NativeWindowResult tab = mock.Create<NativeWindowResult>(new NamedParameter("dispatcher", Dispatcher.CurrentDispatcher));
                Test_can_execute_always_true(tab);
            }
        }

        [Test]
        public void Test_execute_calls_Whitelist()
        {
            // Arrange
            var mockTab = new Mock<WindowResult>();
            mockTab.Setup(x => x.IsWhiteListed).Returns(false);
            var cmd = new WhiteListItemCommand();

            // Act
            cmd.Execute(mockTab.Object);

            // Assert
            mockTab.Verify(x => x.Whitelist(), Times.Once);
        }

        [Test]
        public void Test_execute_doesnt_call_Whitelist_when_already_Whitelisted()
        {
            // Arrange
            var mockTab = new Mock<WindowResult>();
            mockTab.Setup(x => x.IsWhiteListed).Returns(true);
            var cmd = new WhiteListItemCommand();

            // Act
            cmd.Execute(mockTab.Object);

            // Assert
            mockTab.Verify(x => x.Whitelist(), Times.Never);
        }

        [Test]
        public void Test_execute_doesnt_error_on_null()
        {
            // Arrange
            var cmd = new WhiteListItemCommand();

            // Act
            // Assert
            Assert.DoesNotThrow(() => cmd.Execute(null));
        }

        [Test]
        public void Test_execute_on_incorrect_input_throws_exception()
        {
            // Arrange
            var mockTab = new Mock<WindowResult>();
            var cmd = new WhiteListItemCommand();

            // Act
            Assert.That(() => cmd.Execute("foo"), Throws.InstanceOf<InvalidCastException>());

            // Assert
            mockTab.Verify(x => x.Whitelist(), Times.Never);
        }

    }
}
