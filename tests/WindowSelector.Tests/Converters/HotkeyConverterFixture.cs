using System;
using System.Globalization;
using System.Windows.Input;
using NUnit.Framework;
using WindowSelector.Common.Configuration;
using WindowSelector.Converters;

namespace WindowSelector.Tests.Converters
{
    [TestFixture]
    public class HotkeyConverterFixture
    {
        [Test]
        public void Test_valid_string_returned_for_valid_input()
        {
            // Arrange
            var converter = new HotkeyConverter();
            var hotkey = new HotkeySetting(Key.B, ModifierKeys.Windows | ModifierKeys.Shift);

            // Act
            var result = converter.Convert(hotkey, typeof(string), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.That(result, Contains.Substring("Windows"));
            Assert.That(result, Contains.Substring("Shift"));
            Assert.That(result, Contains.Substring("+ B"));
        }

        [Test]
        [TestCase(Key.OemTilde, "`")]
        [TestCase(Key.OemQuestion, "/")]
        [TestCase(Key.NumPad5, "NUM5")]
        [TestCase(Key.D5, "5")]
        [TestCase(Key.Multiply, "NUM*")]
        public void Test_valid_string_for_special_cases(Key key, string expected)
        {
            // Arrange
            var converter = new HotkeyConverter();
            var hotkey = new HotkeySetting(key, ModifierKeys.None);

            // Act
            var result = converter.Convert(hotkey, typeof(string), null, CultureInfo.CurrentCulture);

            //Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Test_valid_string_returned_for_valid_input_no_modifiers()
        {
            // Arrange
            var converter = new HotkeyConverter();
            var hotkey = new HotkeySetting(Key.B, ModifierKeys.None);

            // Act
            var result = converter.Convert(hotkey, typeof(string), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.That(result, Is.EqualTo("B"));
        }

        [Test]
        public void Test_null_returned_for_invalid_input()
        {
            var converter = new HotkeyConverter();
            var notHotkey = new object();

            var result = converter.Convert(notHotkey, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Test_convert_back_throws()
        {
            var converter = new HotkeyConverter();

            Assert.That(() => converter.ConvertBack("Control + Shift + B", typeof(HotkeySetting), null, CultureInfo.CurrentCulture), Throws.InstanceOf<NotImplementedException>());
        }
    }
}
