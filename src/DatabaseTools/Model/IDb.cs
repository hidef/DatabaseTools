using System.Collections.Generic;

namespace DatabaseTools.Model
{
    internal interface IDb
    {
        IEnumerable<Table> TableTypes { get; }
        void Apply(DbDiff diff);
        string GenerateScript(DbDiff diff);
    }
}