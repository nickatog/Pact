namespace Pact
{
    public struct DeckImportDetails
    {
        public DeckImportDetails(
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
