namespace DatabaseTools.Model
{
    internal interface IDb
    {
        DatabaseModel GetModel();
        void Apply(DbDiff diff);
        string GenerateScript(DbDiff diff);
    }
}