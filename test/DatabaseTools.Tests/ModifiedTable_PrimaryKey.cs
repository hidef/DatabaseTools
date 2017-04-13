using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests
{
    public class ModifiedTable_PrimaryKey
    {
        [Fact]
        public void AddedPrimaryKey()
        {
            var old = new Table {
                Name = "a new table",
            };

            var @new = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "UserId" }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.True(diff.ModifiedTables.Single().IsModified);
            Assert.True(diff.ModifiedTables.Single().IsPrimaryKeyAdded);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyChanged);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyRemoved);
        }

        [Fact]
        public void ChangedPrimaryKey()
        {
            var old = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "UserId" }
            };

            var @new = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "GroupId", "UserId" }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.True(diff.ModifiedTables.Single().IsModified);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyAdded);
            Assert.True(diff.ModifiedTables.Single().IsPrimaryKeyChanged);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyRemoved);
        }

        [Fact]
        public void RemovedPrimaryKey()
        {
            var old = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "UserId" }
            };

            var @new = new Table {
                Name = "a new table",
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.True(diff.ModifiedTables.Single().IsModified);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyAdded);
            Assert.False(diff.ModifiedTables.Single().IsPrimaryKeyChanged);
            Assert.True(diff.ModifiedTables.Single().IsPrimaryKeyRemoved);
        }
    }
}
