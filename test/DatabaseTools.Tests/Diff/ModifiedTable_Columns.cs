using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests.Diff
{
    public class ModifiedTable_Columns
    {
        [Fact]
        public void AddedColumns()
        {
            var old = new Table {
                Name = "a new table",
                Fields = new [] {
                    new Field {
                        Name = "UserId",
                        Type = "int"
                    }
                }
            };

            var @new = new Table {
                Name = "a new table",
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(1, diff.ModifiedTables.Single().AddedColumns.Count());
            Assert.Equal("UserId", diff.ModifiedTables.Single().AddedColumns.Single().Name);
            Assert.Equal("int", diff.ModifiedTables.Single().AddedColumns.Single().Type);
        }

        [Fact]
        public void RemovedColumns()
        {
            var old = new Table {
                Name = "a new table",
            };

            var @new = new Table {
                Name = "a new table",
                Fields = new [] {
                    new Field {
                        Name = "UserId",
                        Type = "int"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(1, diff.ModifiedTables.Single().RemovedColumns.Count());
            Assert.Equal("UserId", diff.ModifiedTables.Single().RemovedColumns.Single().Name);
            Assert.Equal("int", diff.ModifiedTables.Single().RemovedColumns.Single().Type);
        }

        [Fact]
        public void ModifiedColumns() 
        {

            var old = new Table {
                Name = "a new table",
                Fields = new [] {
                    new Field {
                        Name = "UserId",
                        Type = "int"
                    }
                }
            };

            var @new = new Table {
                Name = "a new table",
                Fields = new [] {
                    new Field {
                        Name = "UserId",
                        Type = "string"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(1, diff.ModifiedTables.Single().ChangedColumns.Count());
            Assert.Equal(old.Fields.Single(), diff.ModifiedTables.Single().ChangedColumns.Single().A);
            Assert.Equal(@new.Fields.Single(), diff.ModifiedTables.Single().ChangedColumns.Single().B);
        }
    }
}
