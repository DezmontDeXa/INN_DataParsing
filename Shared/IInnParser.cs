namespace Shared
{
    public interface IInnParser<TData> where TData : IParsedData
    {
        TData Parse(string inn);
    }
}