using System;
using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests.Diff
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
        public void UnChangedPrimaryKey()
        {
            int i = DateTime.Now.Day;

            var old = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "UserId" + i } // Force this string to be a different instance despite having the same value
            };

            var @new = new Table {
                Name = "a new table",
                PrimaryKey = new [] { "UserId" + i } // Force this string to be a different instance despite having the same value
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(0, diff.ModifiedTables.Count);
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
