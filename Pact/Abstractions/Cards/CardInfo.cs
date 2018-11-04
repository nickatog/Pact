namespace Pact
{
    public struct CardInfo
    {
        public string Class { get; }
        public int Cost { get; }
        public int DatabaseID { get; }
        public string ID { get; }
        public string Name { get; }

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
