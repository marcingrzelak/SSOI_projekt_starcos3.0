namespace Eportmonetka.SET_Lib
{
    class TransacItem
    {
        public string Name { get; private set; }
        public short Quantity { get; private set; }
        public float Cost { get; private set; }

        public TransacItem(string N, short Q, float C)
        {
            Name = N;
            Quantity = Q;
            Cost = C;
        }
    }
}
