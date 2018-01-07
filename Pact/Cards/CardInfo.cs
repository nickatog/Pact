namespace Pact
{
    public struct CardInfo
    {
        public string Class { get; private set; }
        public int Cost { get; private set; }
        public int DatabaseID { get; private set; }
        public string ID { get; private set; }
        public string Name { get; private set; }

        public CardInfo(
            string name,
            string @class,
            int cost,
            string id,
            int databaseID)
        {
            Class = @class;
            Cost = cost;
            DatabaseID = databaseID;
            ID = id;
            Name = name;
        }
    }
}
