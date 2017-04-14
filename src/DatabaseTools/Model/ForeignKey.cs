namespace DatabaseTools.Model
{
    public class ForeignKey
    {
        public string Name { get; set; }
        public string LocalColumn { get; set; }
        public string RemoteTable { get; set; }
        public string RemoteColumn { get; set; }
    }
}