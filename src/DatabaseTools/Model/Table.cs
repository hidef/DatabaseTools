using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Table
    {
        public string Name { get; set; }
        public Index PrimaryKey { get; set; }
        public IEnumerable<Index> Indices => new Index[]{};
        public IEnumerable<Field> Fields { get; set; }
    }
}