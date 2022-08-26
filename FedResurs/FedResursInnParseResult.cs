using HtmlAgilityPack;
using Shared;

namespace FedResurs
{
    public class FedResursInnParseResult : ParsedDataBase
    {
        private const string UrlForAttachedFiles = "https://old.bankrot.fedresurs.ru/";
        public FedResursInnParseResult(string inn, Exception ex)
        {
            Inn = inn;
            Exception = ex;
        }
        public FedResursInnParseResult(string inn)
        {
            Inn = inn;
            IsDebtor = false;
        }        
        public FedResursInnParseResult(string inn, string cardId, string docId, string docText, int? _actsCount) : this(inn)
        {
            CardId = cardId;
            DocId = docId;
            DocHTML = docText;
            IsDebtor = true;
            ActsCount = _actsCount;

            ParseHTML();
        }

        private void ParseHTML()
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(DocHTML);
                // //td[text()[contains(.,'ФИО')]]/following-sibling::td[1]

                ActPublishDate = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Дата публикации"))?.InnerText?.Clear();

                FIO = doc.DocumentNode.SelectSingleNode(GetXPathForTable("ФИО должника"))?.InnerText?.Clear();
                Birthday = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Дата рождения"))?.InnerText?.Clear();
                Place = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Место жительства"))?.InnerText?.Clear();
                SNILS = doc.DocumentNode.SelectSingleNode(GetXPathForTable("СНИЛС"))?.InnerText?.Clear();
                PrevFIO = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Ранее имевшиеся ФИО"))?.InnerText?.Clear();
                CaseNo = doc.DocumentNode.SelectSingleNode(GetXPathForTable("№ дела"))?.InnerText?.Clear();

                Arbitr = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Арбитражный управляющий"))?.InnerText?.Clear();
                CorrepondentAddress = doc.DocumentNode.SelectSingleNode(GetXPathForTable("Адрес для корреспонденции"))?.InnerText?.Clear();
                SPOAY = doc.DocumentNode.SelectSingleNode(GetXPathForTable("СРО АУ"))?.InnerText?.Clear();

                Court = doc.DocumentNode.SelectSingleNode("//table[@class='coutInfo'][1]/tr[2]/td[1]")?.InnerText?.Clear();
                ResolutionDate = doc.DocumentNode.SelectSingleNode("//table[@class='coutInfo'][1]/tr[2]/td[3]")?.InnerText?.Clear();
                Text = doc.DocumentNode.SelectSingleNode("//div[@class='msg']")?.InnerText;

                var nodes = doc.DocumentNode.SelectNodes("//div[@class='files']//a");
                if (nodes != null)
                {
                    var hrefs = nodes.Select(x => UrlForAttachedFiles + x.GetAttributeValue("href", "").Replace("&amp;", "&"));
                    AttachedFiles = String.Join(";", hrefs);
                }

                HTMLParsedSuccessfully = true;

            }
            catch(Exception ex)
            {
                HtmlParseException = ex;
                HTMLParsedSuccessfully = false;
            }

            return;
        }

        private string GetXPathForTable(string rowName)
        {
            return $"//td[text()[contains(.,'{rowName}')]]/following-sibling::td[1]";
        }

        public string? CardId { get; }
        public string? DocId { get; }
        public string CardLink => $"https://old.bankrot.fedresurs.ru/PrivatePersonCard.aspx?ID={CardId}";
        public string DocLink => $"https://old.bankrot.fedresurs.ru/MessageWindow.aspx?ID={DocId}";

    }
}