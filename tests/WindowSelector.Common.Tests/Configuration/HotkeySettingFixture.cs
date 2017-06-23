using System.Windows.Input;
using NUnit.Framework;
using WindowSelector.Common.Configuration;

namespace WindowSelector.Tests.Configuration
{
    [TestFixture]
    public class HotkeySettingFixture
    {
        [Test]
        public void Test_HotkeySetting()
        {
            // Arrange
            var hotkey = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);

            // Assert
            Assert.That(hotkey.Key, Is.EqualTo(Key.Z));
            Assert.That(hotkey.Modifiers.HasFlag(ModifierKeys.Alt), Is.True);
            Assert.That(hotkey.Modifiers.HasFlag(ModifierKeys.Control), Is.True);
        }

        [Test]
        public void Test_Equals_matches_equal_objects()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);

            // Assert
            Assert.That(hotkey1, Is.EqualTo(hotkey2));
        }

        [Test]
        public void Test_Equals_false_when_different_key()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.X, ModifierKeys.Control | ModifierKeys.Alt);

            // Assert
            Assert.That(hotkey1, Is.Not.EqualTo(hotkey2));
        }

        [Test]
        public void Test_Equals_false_when_different_modifiers()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows);

            // Assert
            Assert.That(hotkey1, Is.Not.EqualTo(hotkey2));
        }

        [Test]
        public void Test_Equals_false_when_other_object_wrong_type()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = "not a hotkey";

            // Assert
            Assert.That(hotkey1, Is.Not.EqualTo(hotkey2));
        }

        [Test]
        public void Test_GetHashCode_matches_equal_objects()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);

            // Assert
            Assert.That(hotkey1.GetHashCode(), Is.EqualTo(hotkey2.GetHashCode()));
        }

        [Test]
        public void Test_GetHashCode_false_when_different_key()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.X, ModifierKeys.Control | ModifierKeys.Alt);

            // Assert
            Assert.That(hotkey1.GetHashCode(), Is.Not.EqualTo(hotkey2.GetHashCode()));
        }

        [Test]
        public void Test_GetHashCode_false_when_different_modifiers()
        {
            // Arrange
            var hotkey1 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt);
            var hotkey2 = new HotkeySetting(Key.Z, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows);

            // Assert
            Assert.That(hotkey1.GetHashCode(), Is.Not.EqualTo(hotkey2.GetHashCode()));
        }
    }
}