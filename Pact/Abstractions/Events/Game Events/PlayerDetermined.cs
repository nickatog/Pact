namespace Pact.GameEvents
{
    public sealed class PlayerDetermined
    {
        public PlayerDetermined(
            int playerID)
        {
            PlayerID = playerID;
        }

        public int PlayerID { get; }
    }
}
