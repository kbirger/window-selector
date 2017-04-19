using System;
using System.Globalization;
using System.Windows;
using NUnit.Framework;
using WindowSelector.Converters;

namespace WindowSelector.Tests.Converters
{
    [TestFixture]
    public class NullToVisibilityConverterFixture
    {
        [Test]
        [TestCase("foo", false, Visibility.Visible)]
        [TestCase("foo", true, Visibility.Collapsed)]
        [TestCase(null, false, Visibility.Collapsed)]
        [TestCase(null, true, Visibility.Visible)]

        [TestCase(null, null, Visibility.Collapsed)]
        [TestCase(null, 5, Visibility.Collapsed)]
        [TestCase("foo", null, Visibility.Visible)]
        [TestCase("foo", 5, Visibility.Visible)]
        public void Test_return_values(object value, object parameter, Visibility expected)
        {
            var converter = new NullToVisibilityConverter();

            var result = converter.Convert(value, typeof(Visibility), parameter, CultureInfo.CurrentCulture);

            Assert.That(result, Is.EqualTo(expected));
        }
        [Test]
        [TestCase("not null")]
        [TestCase(null)]
        public void Test_nonbool_case_throws(object value)
        {
            var converter = new NullToVisibilityConverter();

            var result = converter.Convert(value, typeof(Visibility), null, CultureInfo.CurrentCulture);

            if(value == null)
                Assert.That(result, Is.EqualTo(Visibility.Collapsed));
            else
                Assert.That(result, Is.EqualTo(Visibility.Visible));
        }


        [Test]
        public void Test_convert_back_throws()
        {
            var converter = new NullToVisibilityConverter();

            Assert.That(() => converter.ConvertBack("anything", typeof(object), null, CultureInfo.CurrentCulture), Throws.InstanceOf<NotImplementedException>());
        }



    }
}
