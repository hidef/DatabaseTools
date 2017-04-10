namespace DatabaseTools.Model
{
    public class IndexModification
    {
        private readonly Index a;
        private readonly Index b;

        public IndexModification(Index a, Index b)
        {
            this.a = a;
            this.b = b;
        }

        public Index A { get { return this.a; } }
        public Index B { get { return this.b; } }
    }
}