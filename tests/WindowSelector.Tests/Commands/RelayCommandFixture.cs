using System;
using NUnit.Framework;
using WindowSelector.Commands;

namespace WindowSelector.Tests.Commands
{
    [TestFixture]
    public class RelayCommandFixture
    {
        [Test]
        public void Test_RelayCommand_execute_null_throws_exception()
        {
            Assert.That(() => new RelayCommand(null, null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Test_CanExecute(bool canExecute)
        {
            // Arrange
            Action<object> executeAction = (o) => { };
            Predicate<object> canExecuteAction = (o) => canExecute;
            var cmd = new RelayCommand(executeAction, canExecuteAction);

            // Act
            var ret = cmd.CanExecute("");

            // Assert
            Assert.That(ret, Is.EqualTo(canExecute));
        }


        [Test]
        public void Test_CanExecute_in_default_case()
        {
            // Arrange
            Action<object> executeAction = (o) => { };
            var cmd = new RelayCommand(executeAction, null);

            // Act
            var ret = cmd.CanExecute("");

            // Assert
            Assert.That(ret, Is.True);
        }

        [Test]
        public void Test_Execute()
        {
            // Arrange
            bool called = false;
            Action<object> executeAction = (o) => called = true;
            var cmd = new RelayCommand(executeAction, null);

            // Act
            cmd.Execute("");

            // Assert
            Assert.That(called, Is.True);
        }


        [Test]
        public void Test_InvokeCanExecuteChanged()
        {
            // Arrange
            bool called = false;
            Action<object> executeAction = (o) => { };
            Predicate<object> canExecuteAction = (o) => true;
            var cmd = new RelayCommand(executeAction, canExecuteAction);
            cmd.CanExecuteChanged += (sender, args) => called = true;

            // Act
            cmd.InvokeCanExecuteChanged();

            // Assert
            Assert.That(called, Is.True);
        }
    }
}