using DatabaseTools.Model;

namespace DatabaseTools.Tests
{
    public static class TestExtensions
    {
        public static DatabaseModel InDbModel(this Table self)
        {
            return new DatabaseModel {
                Tables = new [] { self }
            };
        }
    }
}
