using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Table
    {
        public string Name { get; set; }
        public string[] PrimaryKey { get; set; }
        
        private IList<Index> _indices = new Index[]{};
        public IList<Index> Indices 
        {
            get { return _indices; }
            set { _indices = value; }
        }
        public IList<Field> Fields { get; set; }
    }
}