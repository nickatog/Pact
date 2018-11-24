namespace Pact
{
    public struct DeckImportResult
    {
        public DeckImportResult(
            string title,
            Models.Client.Decklist decklist)
        {
            Title = title;
            Decklist = decklist;
        }

        public Models.Client.Decklist Decklist { get; }
        public string Title { get; }
    }
}
