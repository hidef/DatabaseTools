using System.Collections.Generic;
using System.Linq;

namespace DatabaseTools.Model
{
    public class TableModification
    {
        
        public string Name => this.Input.Name;

        public Table Input { get; set; }
        public Table New { get; set; }

        public bool IsPrimaryKeyAdded { get; set; }
        public bool IsPrimaryKeyRemoved { get; set; }
        public bool IsPrimaryKeyChanged {  get; set; }

        public IList<Index> AddedIndices  {  get; set; }
        public IList<IndexModification> ChangedIndices  {  get; set; }
        public IList<Index> RemovedIndices  {  get; set; }

        public IList<Field> AddedColumns {  get; set; }
        public IList<ColumnModification> ChangedColumns {  get; set; }
        public IList<Field> RemovedColumns {  get; set; }

        public bool IsModified => 
            this.IsPrimaryKeyAdded || 
            this.IsPrimaryKeyChanged || 
            this.IsPrimaryKeyRemoved || 
             this.AddedColumns.Count() +
             this.ChangedColumns.Count() + 
             this.RemovedColumns.Count() 
             > 0;
    }
}