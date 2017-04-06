using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Table
    {
        public string Name { get; set; }
        
        public IEnumerable<Field> Fields { get; set; }
    }

    internal interface IDb
    {
        IEnumerable<Table> TableTypes { get; }
        void Apply(DbDiff diff);
        string GenerateScript(DbDiff diff);
    }
}