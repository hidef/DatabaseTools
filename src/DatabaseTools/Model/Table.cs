using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Table
    {
        public string Name { get; set; }
        public string[] PrimaryKey { get; set; }
        
        private IEnumerable<Index> _indices = new Index[]{};
        public IEnumerable<Index> Indices 
        {
            get { return _indices; }
            set { _indices = value; }
        }
        public IEnumerable<Field> Fields { get; set; }
    }
}