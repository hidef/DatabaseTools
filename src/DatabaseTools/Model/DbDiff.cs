using System.Collections.Generic;

namespace DatabaseTools.Model
{
    public class DbDiff
    {
        public IList<Table> AddedTables { get; internal set; }
        public IList<Table> RemovedTables { get; internal set; }
        public IList<TableModification> ModifiedTables { get; set; }
    }
}