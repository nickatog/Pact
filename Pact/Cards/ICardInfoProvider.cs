namespace Pact
{
    public interface ICardInfoProvider
    {
        CardInfo? GetCardInfo(
            string cardID);

        CardInfo? GetCardInfo(
            int databaseID);
    }
}
