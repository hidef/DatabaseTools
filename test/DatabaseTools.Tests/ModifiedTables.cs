using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests
{
    public class ModifiedTables
    {
        [Fact]
        public void AddedColumns()
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
            Assert.Equal(1, diff.ModifiedTables.Single().AddedColumns.Count());
            Assert.Equal("UserId", diff.ModifiedTables.Single().AddedColumns.Single().Name);
            Assert.Equal("int", diff.ModifiedTables.Single().AddedColumns.Single().Type);
        }

        [Fact]
        public void RemovedColumns()
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
            Assert.Equal(1, diff.ModifiedTables.Single().RemovedColumns.Count());
            Assert.Equal("UserId", diff.ModifiedTables.Single().RemovedColumns.Single().Name);
            Assert.Equal("int", diff.ModifiedTables.Single().RemovedColumns.Single().Type);
        }

        [Fact]
        public void AddedIndices()
        {
            var old = new Table {
                Name = "a new table",
            };

            var @new = new Table {
                Name = "a new table",
                Indices = new [] {
                    new Index {
                        Name = "IX_UserId",
                        Fields = new [] { "UserId" },
                        IsUnique = true
                    }
                },
                Fields = new [] { new Field { } }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(1, diff.ModifiedTables.Single().AddedIndices.Count());
            Assert.Equal("IX_UserId", diff.ModifiedTables.Single().AddedIndices.Single().Name);
            Assert.Equal("UserId", diff.ModifiedTables.Single().AddedIndices.Single().Fields.Single());
            Assert.Equal(true, diff.ModifiedTables.Single().AddedIndices.Single().IsUnique);
        }

        [Fact]
        public void RemovedIndices()
        {
            var old = new Table {
                Name = "a new table",
                Indices = new [] {
                    new Index {
                        Name = "IX_UserId",
                        Fields = new [] { "UserId" },
                        IsUnique = true
                    }
                },
            };

            var @new = new Table {
                Name = "a new table",
                Fields = new [] { new Field { } }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(1, diff.ModifiedTables.Single().RemovedIndices.Count());
            Assert.Equal("IX_UserId", diff.ModifiedTables.Single().RemovedIndices.Single().Name);
            Assert.Equal("UserId", diff.ModifiedTables.Single().RemovedIndices.Single().Fields.Single());
            Assert.Equal(true, diff.ModifiedTables.Single().RemovedIndices.Single().IsUnique);
        }
    }
}
