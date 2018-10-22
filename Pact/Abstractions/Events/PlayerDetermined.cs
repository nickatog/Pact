namespace Pact.Events
{
    public sealed class PlayerDetermined
    {
        public int PlayerID { get; private set; }

        public PlayerDetermined(
            int playerID)
        {
            PlayerID = playerID;
        }
    }
}
