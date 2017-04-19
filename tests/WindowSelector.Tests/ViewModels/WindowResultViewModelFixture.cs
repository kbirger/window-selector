using System.Windows.Threading;
using NUnit.Framework;

namespace WindowSelector.Tests.ViewModels
{
    [TestFixture]
    public class WindowResultViewModelFixture
    {
        private Autofac.Extras.Moq.AutoMock mock;

        [SetUp]
        public void Setup()
        {
            mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            mock.Provide(Dispatcher.CurrentDispatcher);
        }

        [TearDown]
        public void Teardown()
        {
            mock.Dispose();
            mock = null;
        }

        [Test]
        public void Test_Constructor_sets_up_properties()
        {
            // Arrange
            //var minKey = new HotkeySetting(Key.Z, ModifierKeys.None);
            //var closeKey = new HotkeySetting(Key.Y, ModifierKeys.None);
            //var config = new ConfigurationRoot(new Dictionary<string, HotkeySetting>()
            //{
            //    {"minimize_key", minKey},
            //    {"close_key", closeKey},

            //});
            //mock.Mock<IConfigurationProvider>()
            //    .Setup(p => p.GetConfiguration())
            //    .Returns(config);
            //var vm = mock.Create<MainWindowViewModel>();

            //// Assert
            //Assert.That(vm.CloseKey, Is.EqualTo(closeKey));
            //Assert.That(vm.MinimizeKey, Is.EqualTo(minKey));
        }
        [Test]
        public void Test_Reset()
        {
            
        }

        [Test]
        public void Test_Search()
        {

        }

        [Test]
        public void Test_GetEnumerator()
        {

        }
    }
}