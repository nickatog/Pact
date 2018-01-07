namespace Pact.Events
{
    public sealed class HeroEntityIdentified
    {
        public string CardID { get; private set; }
        public int EntityID { get; private set; }
        public int PlayerID { get; private set; }

        public HeroEntityIdentified(
            int playerID,
            int entityID,
            string cardID)
        {
            CardID = cardID;
            EntityID = entityID;
            PlayerID = playerID;
        }
    }
}
