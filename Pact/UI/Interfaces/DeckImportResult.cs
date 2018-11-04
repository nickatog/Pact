namespace Pact
{
    public struct DeckImportResult
    {
        public DeckImportResult(
            string title,
            Decklist decklist)
        {
            Title = title;
            Decklist = decklist;
        }

        public Decklist Decklist { get; }

        public string Title { get; }
    }
}
