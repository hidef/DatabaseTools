using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests
{
    public class ModifiedTable_Indices
    {

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
