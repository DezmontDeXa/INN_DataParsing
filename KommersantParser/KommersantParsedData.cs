using HtmlAgilityPack;
using Shared;

namespace Kommersant
{
    public class KommersantParsedData : ParsedDataBase
    {
        public KommersantParsedData(string inn, Exception ex)
        {
            Inn = inn;
            Exception = ex;
        }

        public KommersantParsedData(string inn, string hash, string html)
        {
            DocHTML = html;
            InnUrl = $"https://bankruptcy.kommersant.ru/search/poisk_soobshcheniya_o_bankrotstve/{hash}/";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var searchResultCount = doc.DocumentNode.SelectSingleNode("//p[@class='search-count']")?.InnerText;
            var pseudops = doc.DocumentNode.SelectNodes("//div[@class='pseudop']");
            if (searchResultCount == null ||  searchResultCount == "По запросу ничего не найдено" || pseudops == null)
            {
                IsDebtor = false;
                return;
            }

            ActPublishDate = doc.DocumentNode.SelectSingleNode("//h1[@class='article_subheader']")?.InnerText;

            foreach (var pseudop in pseudops)
                Text += pseudop.InnerText + "\r\n";
            IsDebtor = true;
        }

        public string? InnUrl { get; }
    }
}