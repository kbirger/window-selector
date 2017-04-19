using System;
using NUnit.Framework;
using WindowSelector.Common;

namespace WindowSelector.Tests
{
    [TestFixture]
    public class ExtensionsFixture
    {
        [Test]
        public void Test_In_returns_true_when_exists()
        {
            Assert.That(5.In(1, 2, 3, 4, 5), Is.True);
            Assert.That("5".In("1","2","3","4","5"), Is.True);
        }

        [Test]
        public void Test_In_returns_false_when_not_exists()
        {
            Assert.That(5.In(1, 2, 3, 4, 6), Is.False);
            Assert.That("5".In("1", "2", "3", "4", "6"), Is.False);
            Assert.That("5".In(), Is.False);
        }

        [Test]
        public void Test_ContainsCaseInsensitive_returns_true_when_match()
        {
            Assert.That("FoO".ContainsCaseInsensitive("foo"));
        }

        [Test]
        public void Test_ContainsCaseInsensitive_returns_false_when_no_match()
        {
            Assert.That("FoO".ContainsCaseInsensitive("foot"), Is.False);
        }

        [Test]
        public void Test_ContainsCaseInsensitive_returns_false_when_null()
        {
            Assert.That(((string)null).ContainsCaseInsensitive("foot"), Is.False);
        }

        [TestCase(5, 1, 10, true)]
        [TestCase(5, 1, 5, true)]
        [TestCase(5, 1, 5, true)]
        [TestCase(1, 1, 5, true)]
        [TestCase(0, 1, 5, false)]
        [TestCase(6, 1, 5, false)]
        public void Test_InRange_cases(IComparable candidate, IComparable min, IComparable max, bool result)
        {
            Assert.That(candidate.InRange(min, max), Is.EqualTo(result));
        }
    }
}
