using System.IO;
using System.Text;
using SystemInterface.IO;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Windows.Input;
using WindowSelector.Common.Configuration;

namespace WindowSelector.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationRootFixture
    {
        [Test]
        public void Test_configuration_load_returns_configurationroot()
        {
            // Arrange
            ConfigurationRoot actual = new ConfigurationRoot();
            actual.Blacklist.Add("foo");
            string actualString = JsonConvert.SerializeObject(actual);

            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.OpenText("config.json"))
                .Returns(
                    Mock.Of<IStreamReader>(
                        sw =>
                            sw.StreamReaderInstance ==
                            new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(actualString)))));

            
            // Act
            var config = ConfigurationRoot.Load(fileMock.Object);

            // Assert
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Blacklist.Contains("foo"));
        }

        [Test]
        public void Test_configuration_load_returns_new_configurationroot_when_file_doesnt_exist()
        {
            // Arrange
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.OpenText("config.json"))
                .Throws<FileNotFoundException>();

            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));

            // Act
            var config = ConfigurationRoot.Load(fileMock.Object);

            // Assert
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Blacklist.Contains("foo"), Is.False);
        }


        [Test]
        public void Test_configuration_load_returns_new_configurationroot_when_file_corrupted()
        {
            // Arrange
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.OpenText("config.json"))
                .Throws<JsonSerializationException>();

            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));

            // Act
            var config = ConfigurationRoot.Load(fileMock.Object);

            // Assert
            fileMock.Verify(f => f.Delete("config.json"), Times.Once);
            fileMock.Verify(f => f.CreateText("config.json"), Times.Once);

            Assert.That(config, Is.Not.Null);
            Assert.That(config.Blacklist.Contains("foo"), Is.False);
        }

        [Test]
        public void Test_configuration_save_returns_empty_list_when_no_changes()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));

            // Act
            var ret = config.Save(fileMock.Object);

            // assert 
            Assert.That(ret, Is.Empty);

        }

        [Test]
        public void Test_configuration_save_returns_correct_list_when_no_changed()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));

            // Act
            config.Hotkeys["activation_key"] = new HotkeySetting(Key.A, ModifierKeys.None);
            var ret = config.Save(fileMock.Object);

            // assert 
            Assert.That(ret, Has.Length.EqualTo(1));
            Assert.That(ret, Contains.Item("Hotkeys[\"activation_key\"]"));
        }

        [Test]
        public void Test_AddToBlacklist_sets_dirty_and_adds()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            // Act
            config.Blacklist.Add("foo");
            var dirtyFields = config.Save(fileMock.Object);
            // Assert
            Assert.That(config.Blacklist, Is.EqualTo(new[] { "foo" }), "Blacklist should contain only 'foo'");
            Assert.That(dirtyFields, Is.EqualTo(new [] { "Blacklist"}), "Only 'Blacklist' should be in dirty");
        }

        [Test]
        public void Test_RemoveFromBlacklist_removes()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Blacklist.Add("foo");
            config.Blacklist.Add("bar");
            // Act
            config.Blacklist.Remove("foo");

            config.Save(fileMock.Object);
            // Assert
            Assert.That(config.Blacklist, Is.EqualTo(new[] { "bar" }));
        }

        [Test]
        public void Test_RemoveFromBlacklist_does_not_set_dirty_when_item_not_found()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Blacklist.Add("foo");
            config.Save(fileMock.Object);
                        
            // Act
            config.Blacklist.Remove("bar");
            var dirtyFields = config.Save(fileMock.Object);

            // Assert
            Assert.That(dirtyFields, Is.Empty);
        }

        [Test]
        public void Test_AddToBlacklist_does_not_set_dirty_when_item_not_found()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Blacklist.Add("foo");
            config.Save(fileMock.Object);

            // Act
            config.Blacklist.Add("foo");
            var dirtyFields = config.Save(fileMock.Object);

            // Assert
            Assert.That(dirtyFields, Is.Empty);
        }

        [Test]
        public void Test_AddToWhitelist_sets_dirty_and_adds()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            // Act
            config.Whitelist.Add("foo");
            var dirtyFields = config.Save(fileMock.Object);
            // Assert
            Assert.That(config.Whitelist, Is.EqualTo(new[] { "foo" }), "Whitelist should contain only 'foo'");
            Assert.That(dirtyFields, Is.EqualTo(new[] { "Whitelist" }), "Only 'Whitelist' should be in dirty");
        }

        [Test]
        public void Test_RemoveFromWhitelist_removes()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Whitelist.Add("foo");
            config.Whitelist.Add("bar");
            // Act
            config.Whitelist.Remove("foo");

            config.Save(fileMock.Object);
            // Assert
            Assert.That(config.Whitelist, Is.EqualTo(new[] { "bar" }));
        }

        [Test]
        public void Test_RemoveFromWhitelist_does_not_set_dirty_when_item_not_found()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Whitelist.Add("foo");
            config.Save(fileMock.Object);

            // Act
            config.Whitelist.Remove("bar");
            var dirtyFields = config.Save(fileMock.Object);

            // Assert
            Assert.That(dirtyFields, Is.Empty);
        }

        [Test]
        public void Test_AddToWhitelist_does_not_set_dirty_when_item_not_found()
        {
            // Arrange
            var config = new ConfigurationRoot();
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.CreateText(("config.json")))
                .Returns(
                    Mock.Of<IStreamWriter>(sw => sw.StreamWriterInstance == StreamWriter.Null));
            config.Whitelist.Add("foo");
            config.Save(fileMock.Object);

            // Act
            config.Whitelist.Add("foo");
            var dirtyFields = config.Save(fileMock.Object);

            // Assert
            Assert.That(dirtyFields, Is.Empty);
        }
    }
}
