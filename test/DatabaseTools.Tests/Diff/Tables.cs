using System.Linq;
using DatabaseTools.Model;
using Xunit;

namespace DatabaseTools.Tests.Diff
{
    public class Tables
    {
        [Fact]
        public void Add()
        {
            var input = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "MyNewTable"
                    }
                }
            };

            var output = new DatabaseModel {
                Tables = new Table[] {}
            };

            var diff = new DiffGenerator().Diff(input, output);

            Assert.Equal(0, diff.RemovedTables.Count());
            Assert.Equal(1, diff.AddedTables.Count());
            Assert.Equal("MyNewTable", diff.AddedTables.Single().Name);
        }

        [Fact]
        public void Remove()
        {
            var input = new DatabaseModel {
                Tables = new Table[] {}
            };

            var output = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "MyExistingTable"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(input, output);

            Assert.Equal(0, diff.AddedTables.Count());
            Assert.Equal(1, diff.RemovedTables.Count());
            Assert.Equal("MyExistingTable", diff.RemovedTables.Single().Name);
        }

        [Fact]
        public void AddAndRemove()
        {
            var input = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "MyNewTable"
                    }
                }
            };

            var output = new DatabaseModel {
                Tables = new Table[] {
                    new Table {
                        Name = "MyOldTable"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(input, output);

            Assert.Equal("MyNewTable", diff.AddedTables.Single().Name);
            Assert.Equal("MyOldTable", diff.RemovedTables.Single().Name);
        }

        [Fact]
        public void NothingChanged()
        {
            var input = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "ATable"
                    }
                }
            };

            var output = new DatabaseModel {
                Tables = new Table[] {
                    new Table {
                        Name = "ATable"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(input, output);

            Assert.Equal(0, diff.AddedTables.Count());
            Assert.Equal(0, diff.ModifiedTables.Count());
            Assert.Equal(0, diff.RemovedTables.Count());
        }
        
        [Fact]
        public void TableNamesAreCaseInsensitive()
        {
            var input = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "sometable"
                    }
                }
            };

            var output = new DatabaseModel {
                Tables = new Table[] {
                    new Table {
                        Name = "SomeTable"
                    }
                }
            };

            var diff = new DiffGenerator().Diff(input, output);

            Assert.Equal(0, diff.AddedTables.Count());
            Assert.Equal(0, diff.ModifiedTables.Count());
            Assert.Equal(0, diff.RemovedTables.Count());
        }
    }
}
