namespace Pact
{
    public interface ICardInfoProvider
    {
        // convert to Task<CardInfo?> for both
        CardInfo? GetCardInfo(
            string cardID);

        CardInfo? GetCardInfo(
            int databaseID);
    }
}
