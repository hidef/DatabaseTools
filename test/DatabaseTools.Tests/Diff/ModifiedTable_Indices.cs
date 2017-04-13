using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests.Diff
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


        [Fact]
        public void ChangeIndices() 
        {

            var old = new Table {
                Name = "a new table",
                Indices = new [] {
                    new Index {
                        Name = "IX_1",
                        Fields = new [] { "UserId" },
                        IsUnique = true
                    },
                    new Index {
                        Name = "IX_2",
                        Fields = new [] { "UserId" },
                        IsUnique = true
                    }
                }
            };

            var @new = new Table {
                Name = "a new table",
                Indices = new [] {
                    new Index {
                        Name = "IX_1",
                        Fields = new [] { "GroupId", "UserId" },
                        IsUnique = true
                    },
                    new Index {
                        Name = "IX_2",
                        Fields = new [] { "UserId" },
                        IsUnique = false
                    }
                },
                Fields = new [] { new Field { } }
            };

            var diff = new DiffGenerator().Diff(old.InDbModel(), @new.InDbModel());

            Assert.Equal(1, diff.ModifiedTables.Count);
            Assert.Equal(true, diff.ModifiedTables.Single().IsModified);
            Assert.Equal(2, diff.ModifiedTables.Single().ChangedIndices.Count());
            
            Assert.Equal(old.Indices[0], diff.ModifiedTables.Single().ChangedIndices[0].A);
            Assert.Equal(@new.Indices[0], diff.ModifiedTables.Single().ChangedIndices[0].B);
            
            Assert.Equal(old.Indices[1], diff.ModifiedTables.Single().ChangedIndices[1].A);
            Assert.Equal(@new.Indices[1], diff.ModifiedTables.Single().ChangedIndices[1].B);
        }
    }
}
