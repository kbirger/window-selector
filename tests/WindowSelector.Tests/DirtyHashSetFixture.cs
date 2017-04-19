using NUnit.Framework;
using System.Linq;
using Moq;
using WindowSelector.Common.Configuration;

namespace WindowSelector.Tests
{
    [TestFixture]
    public class DirtyHashSetFixture
    {
        [Test]
        public void Test_GetEnumerator()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");
                set.Add("bar");
                // Act
                var enumerated = set.ToArray();

                // Assert
                Assert.That(enumerated, Contains.Item("foo"));
                Assert.That(enumerated, Contains.Item("bar"));
            }
        }

        [Test]
        public void Test_Add_new_sets_dirty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                // Act
                set.Add("foo");
                // Assert
                Assert.That(set, Contains.Item("foo"));
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Once);
            }
        }

        [Test]
        public void Test_Add_when_already_exists_doesnt_set_dirty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");
                mock.Mock<IDirtyHandle>().ResetCalls();
                // Act
                set.Add("foo");
                // Assert
                Assert.That(set, Contains.Item("foo"));
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Never);
            }
        }

        [Test]
        public void Test_Clear_empty_doesnt_set_dirty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                // Act
                set.Clear();
                // Assert
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Never);
            }
        }

        [Test]
        public void Test_Clear_non_empty_sets_dirty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");
                mock.Mock<IDirtyHandle>().ResetCalls();
                // Act
                set.Clear();
                // Assert
                Assert.That(set, Is.Empty);
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Once);
            }
        }

        [Test]
        public void Test_Contains()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();

                // Act
                set.Add("foo");
                
                // Assert
                Assert.That(set, Contains.Item("foo"));
                Assert.That(set, Does.Not.Contain("bar"));
            }
        }

        [Test]
        public void Test_CopyTo()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");

                // Act
                string[] dest = new string[1];
                set.CopyTo(dest, 0);
                // Assert
                Assert.That(dest.Length, Is.EqualTo(1));
                Assert.That(dest, Contains.Item("foo"));
            }
        }

        [Test]
        public void Test_Remove_removes_item()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");
                mock.Mock<IDirtyHandle>().ResetCalls();

                // Act
                bool removed = set.Remove("foo");
                // Assert
                Assert.That(removed, Is.True);
                Assert.That(set, Does.Not.Contain("foo"));
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Once);
            }
        }

        [Test]
        public void Test_Remove_doesnt_remove_nonexistant_item_or_flag_dirty()
        {
            using (var mock = Autofac.Extras.Moq.AutoMock.GetLoose())
            {
                // Arrange 
                DirtyHashSet set = mock.Create<DirtyHashSet>();
                set.Add("foo");
                mock.Mock<IDirtyHandle>().ResetCalls();

                // Act
                bool removed = set.Remove("bar");
                // Assert
                Assert.That(removed, Is.False);
                Assert.That(set, Does.Contain("foo"));
                mock.Mock<IDirtyHandle>().Verify(h => h.SetDirty(), Times.Never);
            }
        }
    }
}