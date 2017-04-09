using System;

namespace DatabaseTools.Sources.Code
{
    public class PrimaryKeyAttribute : Attribute 
    {
        private string[] _columns;

        public PrimaryKeyAttribute() 
        {

        }

        public PrimaryKeyAttribute(params string[] columns)
        {
            _columns = columns;
        }

        public string[] Columns { get { return _columns; } }
    }
}