using System;

namespace DatabaseTools.Sources.Code
{
    public class ForeignKeyAttribute : Attribute 
    {
        public string Column { get; private set; }
        public ForeignKeyAttribute(string column)
        {
            this.Column = column;
        }
    }
}