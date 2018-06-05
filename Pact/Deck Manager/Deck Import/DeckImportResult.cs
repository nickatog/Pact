namespace Pact
{
    public struct DeckImportResult
    {
        public Decklist Decklist { get; private set; }
        public string Title { get; private set; }

        public DeckImportResult(
            string title,
            Decklist decklist)
        {
            Decklist = decklist;
            Title = title;
        }
    }
}
