namespace Pact.Models.Interface
{
    public struct DeckImportDetail
    {
        public DeckImportDetail(
            string title,
            Client.Decklist decklist)
        {
            Title = title;
            Decklist = decklist;
        }

        public Client.Decklist Decklist { get; }
        public string Title { get; }
    }
}
