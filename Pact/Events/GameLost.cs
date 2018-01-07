namespace Pact.Events
{
    public sealed class GameLost
    {
        public string EntityName { get; private set; }

        public GameLost(
            string entityName)
        {
            EntityName = entityName;
        }
    }
}
