using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseTools.Model
{
    public static class extensions
    {
        public static bool EqualTo<T>(this IList<T> self, IList<T> other) where T : class
        {
            if ( self == other ) return true;
            
            if ( self == null ) return false;
            if ( other == null ) return false;
            
            if ( self.Count() != other.Count() ) return false;

            int count = self.Count();
            for ( int i = 0; i < count; i++ )
            {
                if ( self[i] != other[i] ) return false;
            }

            return true;
        }
    }
    public class TableModification
    {
        private Table @in;
        private Table @out;

        public TableModification(Table @in, Table @out)
        {
            this.@in = @in;
            this.@out = @out;
        }

        public string Name { get { return this.@in.Name; } }

        public bool IsPrimaryKeyAdded {
            get {
                return (this.@in.PrimaryKey == null || this.@in.PrimaryKey.Count() == 0) && (this.@out.PrimaryKey != null && this.@out.PrimaryKey.Count() > 0 );
            }
        }
        
        public bool IsPrimaryKeyRemoved {
            get {
                return (this.@out.PrimaryKey == null || this.@out.PrimaryKey.Count() == 0) && (this.@in.PrimaryKey != null && this.@in.PrimaryKey.Count() > 0 );
            }
        }
        
        public bool IsPrimaryKeyChanged { 
            get {
                return !this.IsPrimaryKeyAdded && ! this.IsPrimaryKeyRemoved && !this.@in.PrimaryKey.EqualTo(this.@out.PrimaryKey);
            }
        }

        // public IEnumerable<Index> AddedIndices 
        // {
        //     get {
        //         var _in = this.@in.Indices
        //             .Where(i => i != null)
        //             .ToList();
        //         var _out = this.@out.Indices
        //             .Where(i => i != null)
        //             .ToList(); 

        //         return _in
        //             .Where(t => !_out.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
        //             .ToList();
        //     }
        // }

        // public IEnumerable<Index> RemovedIndices 
        // {
        //     get {
        //         var _in = this.@in.Indices
        //             .Append(this.@in.PrimaryKey)
        //             .Where(i => i != null)
        //             .ToList();
        //         var _out = this.@out.Indices
        //             .Append(this.@out.PrimaryKey)
        //             .Where(i => i != null)
        //             .ToList(); 

                   
        //         return _out
        //             .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
        //             .ToList();
        //     }
        // }
        
        // public IEnumerable<IndexModification> ChangeIndices 
        // {
        //     get {
        //         var _in = this.@in.Indices
        //             .Append(this.@in.PrimaryKey)
        //             .Where(i => i != null)
        //             .ToList();
        //         var _out = this.@out.Indices
        //             .Append(this.@out.PrimaryKey)
        //             .Where(i => i != null)
        //             .ToList(); 

        //         return _in
        //             .Join(_out, i => i.Name, i => i.Name, (a, b) => new IndexModification(a, b))
        //             .Where(iMod => !IndexModification.Equals(iMod.A, iMod.B))
        //             .ToList();
        //     }
        // }

        public IEnumerable<ColumnModification> ChangedColumns
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

        public IEnumerable<Field> AddedColumns
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

        public IEnumerable<Field> RemovedColumns
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

        public Table In { get => @in; set => @in = value; }
        public Table Out { get => @out; set => @out = value; }
    }
}