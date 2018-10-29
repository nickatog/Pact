namespace Pact.Events
{
    public sealed class DeckMoved
    {
        public DeckMoved(
            int sourcePosition,
            int targetPosition)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }

        public int SourcePosition { get; }

        public int TargetPosition { get; }
    }
}
