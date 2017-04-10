using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Index
    {   
        public string Name { get; set; }

        public bool IsUnique { get; set; }      

        public IEnumerable<Field> Fields { get; set; }
    }

    public class Table
    {
        public string Name { get; set; }
        public Index PrimaryKey { get; set; }
        public IEnumerable<Index> Indices { get; set; }
        public IEnumerable<Field> Fields { get; set; }
    }

    internal interface IDb
    {
        IEnumerable<Table> TableTypes { get; }
        void Apply(DbDiff diff);
        string GenerateScript(DbDiff diff);
    }
}