using System;

namespace DatabaseTools
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbIgnoreAttribute : Attribute 
    {

    }
}