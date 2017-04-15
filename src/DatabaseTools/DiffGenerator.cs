using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseTools.Model;

namespace DatabaseTools
{
    public class DiffGenerator
    {
        public DbDiff Diff(DatabaseModel old, DatabaseModel @new)
        {
            var joinedTables = old.Tables.FullOuterJoin(@new.Tables, t => t.Name, t => t.Name, Tuple.Create);

            return new DbDiff
            {
                AddedTables = joinedTables.Where(t => t.Item2 == null).Select(t => t.Item1).ToList(),
                ModifiedTables = joinedTables
                    .Where(t => t.Item1 != null && t.Item2 != null)
                    .Select(t => diffTable(t.Item1, t.Item2))
                    .Where(tm => tm.IsModified)
                    .ToList(),
                RemovedTables = joinedTables.Where(t => t.Item1 == null).Select(t => t.Item2).ToList(),
            };
        }

        private static TableModification diffTable(Table input, Table output)
        
        {
            var isPrimaryKeyAdded = IsPrimaryKeyAdded(input, output);
            var isPrimaryKeyRemoved = IsPrimaryKeyRemoved(input, output);
            return new TableModification
            {
                Old = input,
                New = output,
                IsPrimaryKeyAdded = isPrimaryKeyAdded,
                IsPrimaryKeyChanged = IsPrimaryKeyChanged(input, output, isPrimaryKeyAdded, isPrimaryKeyRemoved),
                IsPrimaryKeyRemoved = isPrimaryKeyRemoved,
                AddedColumns = AddedColumns(input, output),
                ChangedColumns = ChangedColumns(input, output),
                RemovedColumns = RemovedColumns(input, output),
                AddedIndices = AddedIndices(input, output),
                ChangedIndices = ChangeIndices(input, output),
                RemovedIndices = RemovedIndices(input, output)
            };
        }

        public static IList<IndexModification> ChangeIndices(Table old, Table @new)
        {
            if ( old.Indices == null || old.Indices.Count == 0 ) return new IndexModification[] {};
            if ( @new.Indices == null || @new.Indices.Count == 0 ) return new IndexModification[] {};

            var _in = old.Indices
                .Where(i => i != null)
                .ToList();
            var _out = @new.Indices
                .Where(i => i != null)
                .ToList(); 

            return _in
                .Join(_out, i => i.Name, i => i.Name, (a, b) => new IndexModification(a, b))
                .Where(iMod => iMod.A.IsUnique != iMod.B.IsUnique || !iMod.A.Fields.EqualTo(iMod.B.Fields))
                .ToList();
        }

        public static IList<ColumnModification> ChangedColumns(Table old, Table @new)
        {
            if ( old.Fields == null || old.Fields.Count == 0 ) return new ColumnModification[] {};
            if ( @new.Fields == null || @new.Fields.Count == 0 ) return new ColumnModification[] {};

            var _in = old.Fields
                .Where(t => !t.Ignored)
                .ToList();
            var _out = @new.Fields
                .Where(t => !t.Ignored)
                .ToList();

            return _in
                .Join(_out, i => i.Name, i => i.Name, (a, b) => new ColumnModification(a, b))
                .Where(cMod => cMod.A.Type != cMod.B.Type)
                .ToList();
        }

        public static IList<Field> AddedColumns(Table old, Table @new)
        {
            if ( old.Fields == null && @new.Fields == null ) return new Field[] {};
            if ( old.Fields == null || old.Fields.Count == 0 ) return @new.Fields;
            if ( @new.Fields == null || @new.Fields.Count == 0 ) return new Field[] {};

            var _in = old.Fields;
            var _out = @new.Fields;

            return _out
                .Where(t => !t.Ignored)
                .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public static IList<Field> RemovedColumns(Table old, Table @new)
        {
            if ( old.Fields == null && @new.Fields == null ) return new Field[] {};
            if ( old.Fields == null || old.Fields.Count == 0 ) return new Field[] {};
            if ( @new.Fields == null || @new.Fields.Count == 0 ) return old.Fields;

            var _in = old.Fields;
            var _out = @new.Fields;

            return _in
                .Where(t => !t.Ignored)
                .Where(t => !_out.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public static IList<Index> AddedIndices(Table old, Table @new)
        {
            var _in = old.Indices;
            var _out = @new.Indices;

            return _out
                .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase) && t.IsUnique == t2.IsUnique && t.Fields.EqualTo(t2.Fields)))
                .ToList();
        }

        public static IList<Index> RemovedIndices(Table old, Table @new)
        {
            var _in = old.Indices;
            var _out = @new.Indices;

            return _in
                .Where(t => !_out.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase) && t.IsUnique == t2.IsUnique && t.Fields.EqualTo(t2.Fields)))
                .ToList();
        }


        public static bool IsPrimaryKeyAdded(Table old, Table @new)
        {
            return (old.PrimaryKey == null || old.PrimaryKey.Count() == 0) && (@new.PrimaryKey != null && @new.PrimaryKey.Count() > 0);
        }

        public static bool IsPrimaryKeyRemoved(Table old, Table @new)
        {
            return (@new.PrimaryKey == null || @new.PrimaryKey.Count() == 0) && (old.PrimaryKey != null && old.PrimaryKey.Count() > 0);
        }

        public static bool IsPrimaryKeyChanged(Table old, Table @new, bool isPrimaryKeyAdded, bool isPrimaryKeyRemoved)
        {
            return !isPrimaryKeyAdded && !isPrimaryKeyRemoved && !old.PrimaryKey.EqualTo(@new.PrimaryKey);
        }
    }
}
