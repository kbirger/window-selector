using System.Collections.Generic;
using System.Windows.Input;
using NUnit.Framework;
using WindowSelector.Common.Configuration;
using WindowSelector.Tests._Utils;
using WindowSelector.ViewModels;

namespace WindowSelector.Tests.ViewModels
{
    [TestFixture]
    public class SettingsViewModelFixture
    {
        [Test]
        public void Test_attributes_return_correctly()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                var config = new ConfigurationRoot();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.B, ModifierKeys.None);
                mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);
                var vm = mock.Create<SettingsWindowViewModel>();

                // Act
                
                // Assert
                Assert.That(vm.Hotkeys["activation_key"].Key, Is.EqualTo(Key.A));
                Assert.That(vm.Hotkeys["debug_key"].Key, Is.EqualTo(Key.B));

            }
        }

        [Test]
        public void Test_Save_saves()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                var config = new ConfigurationRoot();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.B, ModifierKeys.None);

                mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);
                var vm = mock.Create<SettingsWindowViewModel>();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.Z, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.Y, ModifierKeys.None);
                // Act
                vm.Save();
                // Assert
                Assert.That(config.Hotkeys["activation_key"].Key, Is.EqualTo(Key.Z));
                Assert.That(config.Hotkeys["debug_key"].Key, Is.EqualTo(Key.Y));
                mock.Mock<IConfigurationProvider>().Verify(p => p.SaveConfiguration(config));


            }
        }

        [Test]
        public void Test_fires_PropertyChanged()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                var config = new ConfigurationRoot();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.B, ModifierKeys.None);

                mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);
                var eventSource = mock.RegisterMockConfigurationUpdateEventListener();
                var vm = mock.Create<SettingsWindowViewModel>();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.Z, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.Y, ModifierKeys.None);
                var properties = new List<string>();
                vm.PropertyChanged += (sender, args) => properties.Add(args.PropertyName);
                // Act
                eventSource.Handler.Invoke(null, new ConfigurationProvider.ConfigurationUpdateEventArgs(new[] { "Hotkeys[\"debug_key\"]" }));
                // Assert
                Assert.That(properties.Count, Is.EqualTo(1));
                Assert.That(properties, Contains.Item("Hotkeys[\"debug_key\"]"));

            }
        }

        [Test]
        public void Test_doesnt_fire_PropertyChanged_when_wrong_property_updated()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                var config = new ConfigurationRoot();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.B, ModifierKeys.None);

                mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);
                var vm = mock.Create<SettingsWindowViewModel>();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.Z, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.Y, ModifierKeys.None);

                var properties = new List<string>();
                vm.PropertyChanged += (sender, args) => properties.Add(args.PropertyName);
                // Act
                mock.Mock<IConfigurationProvider>().Raise(c => c.ConfigurationUpdated += null, new ConfigurationProvider.ConfigurationUpdateEventArgs(new[] { "DebugKeyXXXXX" }));

                // Assert
                Assert.That(properties.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void Test_PropertyChanged_doesnt_throw_when_no_subscriber()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                var config = new ConfigurationRoot();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.B, ModifierKeys.None);

                mock.Mock<IConfigurationProvider>().Setup(p => p.GetConfiguration()).Returns(config);
                var vm = mock.Create<SettingsWindowViewModel>();
                config.Hotkeys["activation_key"] = new HotkeySetting(Key.Z, ModifierKeys.None);
                config.Hotkeys["debug_key"] = new HotkeySetting(Key.Y, ModifierKeys.None);

                // Assert
                Assert.That(() =>
                    mock.Mock<IConfigurationProvider>()
                        .Raise(c => c.ConfigurationUpdated += null,
                            new ConfigurationProvider.ConfigurationUpdateEventArgs(new[] {"Hotkeys"})),
                    Throws.Nothing);
            }
        }
    }
}
