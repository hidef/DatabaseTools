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
            return new DbDiff {
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
            var coexistingTables = old.Tables.Select(t => new {
                Old = t,
                New = @new.Tables.SingleOrDefault(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase))
            })
            .Where(p => p.New != null);

            return coexistingTables
                .Select(t => new TableModification(t.Old, t.New))
                .Where(tm => tm.IsPrimaryKeyAdded || tm.IsPrimaryKeyChanged || tm.IsPrimaryKeyRemoved || tm.AddedColumns.Count() + tm.ChangedColumns.Count() + tm.RemovedColumns.Count() > 0)
                .ToList();
        }

        private static IList<Table> getRemovedTables(DatabaseModel old, DatabaseModel @new)
        {

            return @new.Tables
                .Where(t => !old.Tables.Any(t2 => String.Equals(t2.Name, t.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
