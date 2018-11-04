namespace Pact
{
    public struct DeckImportDetails
    {
        public DeckImportDetails(
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
