using DatabaseTools.Model;
using DatabaseTools.Sources.Code;
using Newtonsoft.Json;
using Xunit;

namespace DatabaseTools.Tests.CSharp
{
    [PrimaryKey(nameof(Nation.Id))]
    public class Nation
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [PrimaryKey(nameof(User.GroupId), nameof(User.UserId))]
    public class User
    {
        public int GroupId { get; set; }
        public string UserId { get; set; }
        
        // [Index("IX_Name")]
        public string Name { get; set; }

        // [ForeignKey(nameof(Nation.Id))]
        public string Nationality { get; set; } 
    }
    public class CSharpReader
    {
        

        [Fact]
        public void ReadTable()
        {
            var expectedModel = new DatabaseModel {
                Tables = new [] {
                    new Table {
                        Name = "MyUser",
                        PrimaryKey = new [] { "GroupId", "UserId" },
                        Fields = new [] {
                            new Field { Name = "GroupId", Type = "int32"},
                            new Field { Name = "UserId", Type = "string"},
                            new Field { Name = "Name", Type = "string"},
                            new Field { Name = "Nationality", Type = "string"}
                        },
                        // Indices = new [] {
                        //     new Index { Name = "IX_Name", Fields = new [] { "Name" }
                        // },
                        // ForeignKeys = new [] {
                        //     new ForeignKey { Name = "FK_Nationality_Nation_Id", LocalColumn = "Nationality", RemoteTable = "Nation", RemoteColumn = "Id" }
                        // }
                    },
                    new Table {
                        Name = "Nation",
                        PrimaryKey = new [] { "Id" },
                        Fields = new [] {
                            new Field { Name = "Id", Type = "string" },
                            new Field { Name = "Name", Type = "string" }
                        }
                    }
                }
            };

            var generatedModel = new CSharpDbDefiniton(typeof(Database)).GetModel();
            
            Assert.Equal(
                JsonConvert.SerializeObject(expectedModel, Formatting.Indented), 
                JsonConvert.SerializeObject(generatedModel, Formatting.Indented));
        }
    }
}