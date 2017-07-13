using DatabaseTools.Sources.Code;
using DatabaseTools.Tests.CSharp;

namespace DatabaseTools.Tests
{
    public class Database
    {
        public ITable<User> MyUser { get; set; }
        public ITable<Nation> Nation { get; set; }
    }
}