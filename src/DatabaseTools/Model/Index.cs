using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class Index
    {   
        public string Name { get; set; }

        public bool IsUnique { get; set; }

        public IList<string> Fields { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var other = obj as Index;

            if ( other == null ) return false;

            if ( this.IsUnique != other.IsUnique ) return false;

            if ( !string.Equals(this.Name, other.Name) ) return false;

            if ( this.Fields.EqualTo(other.Fields)) return false;

            // TODO: assert that fields are the same and in the same order

            return true;
        }
    }
}