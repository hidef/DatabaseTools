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
            return old.Tables
                .Where(t => !@new.Tables.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static IList<TableModification> getModifiedTables(DatabaseModel old, DatabaseModel @new)
        {
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
                        IsPrimaryKeyChanged = IsPrimaryKeyChanged(t.Old, t.New, isPrimaryKeyAdded, isPrimaryKeyRemoved)
                        IsPrimaryKeyRemoved = isPrimaryKeyRemoved,
                    };
                })
                .Where(tm => tm.IsModified)
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
        private static IList<Table> getRemovedTables(DatabaseModel old, DatabaseModel @new)
        {

            return @new.Tables
                .Where(t => !old.Tables.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
