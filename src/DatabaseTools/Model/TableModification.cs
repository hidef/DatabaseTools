using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseTools.Model
{
    public class TableModification
    {
        private Table @in;
        private Table @out;

        public TableModification(Table @in, Table @out)
        {
            this.@in = @in;
            this.@out = @out;
        }

        public string Name { get; set; }

        public IEnumerable<ColumnModification> Changes
        {
            get {
                var _in = this.@in.Fields
                    .Where(t => !t.Ignored)
                    .ToList();
                var _out = this.@out.Fields
                    .Where(t => !t.Ignored)
                    .ToList(); 

                return _in
                    .Join(_out, i => i.Name, i => i.Name, (a, b) => new ColumnModification(a, b))
                    .Where(cMod => cMod.A.Type != cMod.B.Type)
                    .ToList();
            }
        }

        public IEnumerable<Field> Added 
        {
            get 
            {
                var _in = this.@in.Fields;
                var _out = this.@out.Fields;

                return _in
                    .Where(t => !t.Ignored)
                    .Where(t => !_out.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        public IEnumerable<Field> Removed 
        {
            get 
            {
                var _in = this.@in.Fields;
                var _out = this.@out.Fields;

                return _out
                    .Where(t => !t.Ignored)
                    .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }    
        }
    }
}