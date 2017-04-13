using System;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests
{
    public static class TestExtensions
    {
        public static DatabaseModel InDbModel(this Table self)
        {
            return new DatabaseModel {
                Tables = new [] { self }
            };
        }
    }
    
    public class DiffTests
    {
        [Fact]
        public void AddedTable()
        {
            var old = new DatabaseModel {};
            var @new = new DatabaseModel {
                Tables = new Table[] {
                    new Table {
                        Name = "a new table"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(old, @new);

            Assert.Equal(1, diff.AddedTables.Count);
        }
    }
}
