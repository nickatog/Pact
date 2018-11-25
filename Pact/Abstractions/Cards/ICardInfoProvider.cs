namespace Pact
{
    public interface ICardInfoProvider
    {
        Models.Client.CardInfo? GetCardInfo(
            string cardID);

        Models.Client.CardInfo? GetCardInfo(
            int databaseID);
    }
}
