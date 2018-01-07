namespace Pact.Events
{
    public sealed class GameWon
    {
        public string EntityName { get; private set; }

        public GameWon(
            string entityName)
        {
            EntityName = entityName;
        }
    }
}
