using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class DbDiff
    {
        public IEnumerable<Table> AddedTables { get; internal set; }
        public IEnumerable<Table> RemovedTables { get; internal set; }
        public IEnumerable<TableModification> ModifiedTables { get; set; }
    }
}