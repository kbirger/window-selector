using NUnit.Framework;
using System.Windows.Input;
using WindowSelector.Common.Configuration;
using WindowSelector.Tests.Utilities;

namespace WindowSelector.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationProviderFixture
    {
        [Test]
        public void Test_event_fire()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // arrange
                mock.PrepareWriteableConfigurationProviderMock();
                var configurationProvider = mock.Create<ConfigurationProvider>();
                var configurationRoot = new ConfigurationRoot();
                string[] changedProperties = new string[0];
                configurationProvider.ConfigurationUpdated += (sender, args) =>
                {
                    changedProperties = args.ChangedProperties;
                };

                // act 
                configurationRoot.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
                configurationProvider.SaveConfiguration(configurationRoot);

                // assert
                Assert.That(changedProperties, Has.Length.EqualTo(1));
                Assert.That(changedProperties, Contains.Item("Hotkeys[\"activation_key\"]"));
            }
        }

        [Test]
        public void Test_event_fire_when_no_subscribers_throws_no_error()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                mock.PrepareWriteableConfigurationProviderMock();
                // arrange
                var configurationProvider = mock.Create<ConfigurationProvider>();
                var configurationRoot = new ConfigurationRoot();


                // act 
                configurationRoot.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);

                // assert
                Assert.That(() => configurationProvider.SaveConfiguration(configurationRoot), Throws.Nothing);
            }
        }

        [Test]
        public void Test_Load()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                mock.PrepareReadableConfigurationProviderMock("{\"hotkeys\":{\"activate_key\":{\"key\":146,\"modifiers\":\"Windows\"},\"debug_key\":{\"key\":146,\"modifiers\":\"Shift, Windows\"},\"minimize_key\":{\"key\":26,\"modifiers\":\"Alt\"},\"close_key\":{\"key\":32,\"modifiers\":\"Alt\"}},\"blacklist\":[],\"whitelist\":[]}");
                
                var configurationProvider = mock.Create<ConfigurationProvider>();

                // Act
                var config = configurationProvider.GetConfiguration();

                // Assert
                Assert.That(config, Is.Not.Null);
            }
        }

        [Test]
        public void Test_Load_empty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange
                mock.PrepareReadableConfigurationProviderMock();
                var configurationProvider = mock.Create<ConfigurationProvider>();

                // Act
                var config = configurationProvider.GetConfiguration();

                // Assert
                Assert.That(config, Is.Not.Null);
            }
        }
    }
}
