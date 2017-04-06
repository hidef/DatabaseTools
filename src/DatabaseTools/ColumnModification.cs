namespace DatabaseTools
{
    public class ColumnModification
    {
        public Field A { get; set; }
        public Field B { get; set; }

        public ColumnModification(Field a, Field b)
        {
            this.A = a;
            this.B = b;
        }
    }
}