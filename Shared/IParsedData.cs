namespace Shared
{
    public interface IParsedData
    {
        string? Inn { get; }
        Exception? Exception { get; }
        string? DocHTML { get; }
        bool? IsDebtor { get; }
        bool? HTMLParsedSuccessfully { get; }
        Exception HtmlParseException { get; }
        string DataUrl { get; }
        string? FIO { get; }
        string? Birthday { get; }
        string? Place { get; }
        string? SNILS { get; }
        public string? PrevFIO { get; }
        string? Arbitr { get; }
        string? CorrepondentAddress { get; }
        string? SPOAY { get; }
        string? Court { get; }
        string? CaseNo { get; }
        string? ResolutionDate { get; }
        string? AttachedFiles { get; }
        string? Text { get; }
        public int? ActsCount { get; }

        public string? ActPublishDate { get; }
    }

    public class ParsedDataBase : IParsedData
    {
        public string? Inn { get; protected set; }
        public Exception? Exception { get; protected set; }

        public string? DocHTML { get; protected set; }

        public bool? IsDebtor { get; protected set; }

        public bool? HTMLParsedSuccessfully { get; protected set; }
        public Exception? HtmlParseException { get; protected set; }
        public string DataUrl { get; protected set; }


        public string? FIO  { get; protected set; }

        public string? Birthday  { get; protected set; }

        public string? Place  { get; protected set; }

        public string? SNILS  { get; protected set; }

        public string? PrevFIO { get; protected set; }

        public string? Arbitr  { get; protected set; }

        public string? CorrepondentAddress  { get; protected set; }

        public string? SPOAY  { get; protected set; }

        public string? Court { get; protected set; }

        public string? CaseNo  { get; protected set; }

        public string? ResolutionDate  { get; protected set; }

        public string? AttachedFiles  { get; protected set; }

        public string? Text  { get; protected set; }

        public int? ActsCount { get; protected set; }

        public string? ActPublishDate { get; protected set; }

    }
}