using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests.Diff
{
    public class TableDiffs
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
            Assert.Equal(@new.Tables.Single().Name, diff.AddedTables.Single().Name);
        }

        [Fact]
        public void RemovedTables()
        {
            var old = new DatabaseModel {
                Tables = new Table[] {
                    new Table {
                        Name = "a new table"
                    }
                }
            };
            var @new = new DatabaseModel {};

            var diff = new DiffGenerator().Diff(old, @new);

            Assert.Equal(1, diff.RemovedTables.Count);
            Assert.Equal(old.Tables.Single().Name, diff.RemovedTables.Single().Name);
        }
    }
}
