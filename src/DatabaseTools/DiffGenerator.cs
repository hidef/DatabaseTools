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
            return new DbDiff
            {
                AddedTables = getAddedTables(old, @new),
                ModifiedTables = getModifiedTables(old, @new),
                RemovedTables = getRemovedTables(old, @new),
            };
        }


        private static IList<Table> getAddedTables(DatabaseModel old, DatabaseModel @new)
        {
            if ( old.Tables == null || old.Tables.Count() == 0 ) return @new.Tables;
            if ( @new.Tables == null || @new.Tables.Count() == 0 ) return new Table[] {};

            return old.Tables
                .Where(t => !@new.Tables.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static IList<Table> getRemovedTables(DatabaseModel old, DatabaseModel @new)
        {
            if ( old.Tables == null || old.Tables.Count() == 0 ) return new Table[] {};
            if ( @new.Tables == null || @new.Tables.Count() == 0 ) return @old.Tables;

            return @new.Tables
                .Where(t => !old.Tables.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static IList<TableModification> getModifiedTables(DatabaseModel old, DatabaseModel @new)
        {
            if ( old.Tables == null || old.Tables.Count() == 0 ) return  new TableModification[] {};
            if ( @new.Tables == null || @new.Tables.Count() == 0 ) return new TableModification[] {};
            
            var coexistingTables = old.Tables.Select(t => new
            {
                Old = t,
                New = @new.Tables.SingleOrDefault(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase))
            })
            .Where(p => p.New != null);

            return coexistingTables
                .Select(t =>
                {
                    var isPrimaryKeyAdded = IsPrimaryKeyAdded(t.Old, t.New);
                    var isPrimaryKeyRemoved = IsPrimaryKeyRemoved(t.Old, t.New);
                    return new TableModification
                    {
                        Old = t.Old,
                        New = t.New,
                        IsPrimaryKeyAdded = isPrimaryKeyAdded,
                        IsPrimaryKeyChanged = IsPrimaryKeyChanged(t.Old, t.New, isPrimaryKeyAdded, isPrimaryKeyRemoved),
                        IsPrimaryKeyRemoved = isPrimaryKeyRemoved,
                        AddedColumns = AddedColumns(t.Old, t.New),
                        ChangedColumns = ChangedColumns(t.Old, t.New),
                        RemovedColumns = RemovedColumns(t.Old, t.New),
                        AddedIndices = AddedIndices(t.Old, t.New),
                        // TODO: ChangedIndices = ChangedIndices(t.Old, t.New),
                        RemovedIndices = RemovedIndices(t.Old, t.New)
                    };
                })
                .Where(tm => tm.IsModified)
                .ToList();
        }


        public static IList<ColumnModification> ChangedColumns(Table old, Table @new)
        {
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
            var _in = old.Fields;
            var _out = @new.Fields;

            return _out
                .Where(t => !t.Ignored)
                .Where(t => !_in.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public static IList<Field> RemovedColumns(Table old, Table @new)
        {
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
