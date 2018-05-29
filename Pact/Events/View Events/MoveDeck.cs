namespace Pact.Events
{
    public sealed class MoveDeck
    {
        public int SourcePosition { get; private set; }
        public int TargetPosition { get; private set; }

        public MoveDeck(
            int sourcePosition,
            int targetPosition)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }
    }
}
