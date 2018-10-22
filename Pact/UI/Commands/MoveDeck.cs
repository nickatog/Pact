namespace Pact.Commands
{
    public sealed class MoveDeck
    {
        public MoveDeck(
            int sourcePosition,
            int targetPosition)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }

        public int SourcePosition { get; private set; }

        public int TargetPosition { get; private set; }
    }
}
