using HtmlAgilityPack;
using Shared;

namespace FedResurs
{
    public class FedResursInnParseResult : ParsedDataBase
    {
        private const string UrlForAttachedFiles = "https://old.bankrot.fedresurs.ru/";

        public string? CardId { get; }
        public string? DocId { get; }
        public string CardLink => $"https://old.bankrot.fedresurs.ru/PrivatePersonCard.aspx?ID={CardId}";
        public string DocLink => $"https://old.bankrot.fedresurs.ru/MessageWindow.aspx?ID={DocId}";

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
        public FedResursInnParseResult(string inn, string cardId, string cardHtml, string docId, string docHtml, int? actsCount) : this(inn)
        {
            CardId = cardId;
            CardHTML = cardHtml;
            DocId = docId;
            DocHTML = docHtml;
            IsDebtor = true;
            ActsCount = actsCount;

            if(CardHTML!=null)
                ParseCardHTML();
            if(DocHTML!=null)
                ParseDocHTML();
        }

        private void ParseDocHTML()
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
            catch (Exception ex)
            {
                HtmlParseException = ex;
                HTMLParsedSuccessfully = false;
            }

            return;
        }

        private void ParseCardHTML()
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(CardHTML);
                // //td[text()[contains(.,'ФИО')]]/following-sibling::td[1]

                ActPublishDate = doc.DocumentNode.SelectSingleNode("//table[@id='ctl00_cphBody_gvMessages']//tr[2]/td")?.InnerText?.Clear();

                FIO = GetFIOFromCard(doc);
                Birthday = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblBirthdate']")?.InnerText?.Clear();
                Place = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblAddress']")?.InnerText?.Clear();
                SNILS = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblSNILS']")?.InnerText?.Clear();
                PrevFIO = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblNameHistory']")?.InnerText?.Clear();

                Messages = doc.DocumentNode.SelectNodes("//table[@id='ctl00_cphBody_gvMessages']//a[@title='Просмотр сообщения']")?.Select(x => x.InnerText?.Clear())?.ToList();

                HTMLParsedSuccessfully = true;
            }
            catch (Exception ex)
            {
                HtmlParseException = ex;
                HTMLParsedSuccessfully = false;
            }

            return;
        }

        private string? GetFIOFromCard(HtmlDocument doc)
        {
            var lastName = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblLastName']")?.InnerText?.Clear();
            var firstName = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblFirstName']")?.InnerText?.Clear();
            var midleName = doc.DocumentNode.SelectSingleNode("//span[@id='ctl00_cphBody_lblMiddleName']")?.InnerText?.Clear();
            return $"{lastName} {firstName} {midleName}";
        }

        private string GetXPathForTable(string rowName)
        {
            return $"//td[text()[contains(.,'{rowName}')]]/following-sibling::td[1]";
        }
    }
}