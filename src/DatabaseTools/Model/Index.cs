using System.Collections.Generic;
using System.Linq;

namespace DatabaseTools.Model
{
    public class Index
    {   
        public string Name { get; set; }

        public bool IsUnique { get; set; }      

        public IEnumerable<Field> Fields { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var other = obj as Index;

            if ( other == null ) return false;

            if ( this.IsUnique != other.IsUnique ) return false;

            if ( !string.Equals(this.Name, other.Name) ) return false;

            if ( this.Fields.Count() != other.Fields.Count()) return false;

            return true;
        }
    }
}