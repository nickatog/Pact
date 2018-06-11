namespace Pact
{
    public struct DeckImportDetails
    {
        public Decklist Decklist { get; private set; }
        public string Title { get; private set; }

        public DeckImportDetails(
            string title,
            Decklist decklist)
        {
            Decklist = decklist;
            Title = title;
        }
    }
}
