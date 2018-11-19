namespace Pact.ViewCommands
{
    public sealed class MoveDeck
    {
        public MoveDeck(
            ushort sourcePosition,
            ushort targetPosition)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }

        public ushort SourcePosition { get; }

        public ushort TargetPosition { get; }
    }
}
