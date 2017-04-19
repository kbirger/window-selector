using System;
using System.Globalization;
using NUnit.Framework;
using WindowSelector.Converters;
using System.Windows.Media;

namespace WindowSelector.Tests.Converters
{
    [TestFixture]
    public class BoolToConnectionBrushConverterFixture
    {
        [Test]
        public void Test_nonbool_case_throws()
        {
            var converter = new BoolToConnectionBrushConverter();

            Assert.That(() => converter.Convert("false", typeof(Brush), null, CultureInfo.CurrentCulture), Throws.InstanceOf<InvalidCastException>());
        }


        [Test]
        [TestCase(true, "#FF008000")]
        [TestCase(false, "#FFFF0000")]
        public void Test_that_returns_correct_brush(bool input, string hexColor)
        {
            var converter = new BoolToConnectionBrushConverter();
            converter.TrueBrush = new SolidColorBrush(Colors.Green);
            converter.FalseBrush = new SolidColorBrush(Colors.Red);
            
            var brush = (SolidColorBrush) converter.Convert(input, typeof(Brush), null, CultureInfo.CurrentCulture);

            Assert.That(brush.Color.ToString(), Is.EqualTo(hexColor));
        }

        [Test]
        public void Test_convert_back_throws()
        {
            var converter = new BoolToConnectionBrushConverter();

            Assert.That(() => converter.ConvertBack(new SolidColorBrush(Colors.Red), typeof(bool), null, CultureInfo.CurrentCulture), Throws.InstanceOf<NotImplementedException>());
        }
    }
}
