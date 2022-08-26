using HtmlAgilityPack;
using RestSharp;
using Shared;
using System.Text.RegularExpressions;

namespace FedResurs
{
    public class FedResursParser<T> : IInnParser<ParsedDataBase> where T : IParsedData
    {
        private const string BaseUrl = "https://old.bankrot.fedresurs.ru";

        private readonly RestClient _restClient;
        private readonly Regex _protectCodeRegex = new Regex("toNumbers\\(\"(?<code>[0-9a-f].+?)\"\\)");
        private readonly Regex _cardIdRegex = new Regex("<a href=\"/PrivatePersonCard\\.aspx\\?ID=(?<id>.+?)\" title");
        private readonly Regex _docIdRegex1 = new Regex("openNewWin\\('/MessageWindow\\.aspx\\?ID=(?<docId>.+?)', 'Сообщение', 'yes', '1000', '600'\\); return false;\">[\r\n\t]+?Сообщение о судебном акте</a>");

        #region unused regexs
        private readonly Regex _viewStateRegex = new Regex("__VIEWSTATE\" value=\"(?<data>.+?==)\" />");
        private readonly Regex _viewStateGeneratorRegex = new Regex("<input type=\"hidden\" name=\"__VIEWSTATEGENERATOR\" id=\"__VIEWSTATEGENERATOR\" value=\"(?<data>.+?)\" />");
        private readonly Regex _previewPageRegex = new Regex("<input type=\"hidden\" name=\"__PREVIOUSPAGE\" id=\"__PREVIOUSPAGE\" value=\"(?<data>.+?)\" />");
        private readonly Regex _formUriRegex = new Regex("<form method=\"post\" action=\"\\./ (?<uri>.+?)\" onsubmit");
        private readonly Regex _docIdRegex = new Regex("onclick=\"openNewWin\\('/MessageWindow\\.aspx\\?ID=(?<docId>.+?)', .+?Сообщение о судебном акте</a>", RegexOptions.Singleline);
        private readonly Regex _docTextRegex = new Regex("<div class=\"msg\"><b>Текст:</b>(?<text>.+?)</div>", RegexOptions.Singleline);
        private readonly Regex _htmlTags = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
        #endregion

        private string _formUri;
        private string _viewState;
        private string _viewStateGenerator;
        private string _previewPage;
        private string _referer;
        private object locker;

        private int? _actsCount = null;

        public FedResursParser()
        {
            locker = new object();
            _restClient = new RestClient(FedResursParser<T>.BaseUrl);
            _restClient.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36");
            CreateSession();
        }

        public ParsedDataBase Parse(string inn)
        {
            lock (locker)
            {
                try
                {
                    var cardId = GetInnCardId(inn);
                    if (cardId == null) return new FedResursInnParseResult(inn);
                    var docId = GetDocId(cardId);
                    var docHtml = GetDocHTML(docId);
                    ResetSession();

                    return new FedResursInnParseResult(inn, cardId, docId, docHtml, _actsCount);
                }
                catch (Exception ex)
                {
                    return new FedResursInnParseResult(inn, ex);
                }
            }
        }

        private void ResetSession()
        {
            var response = _restClient.Get(DebtorsSearchRequest());
            if (!response.IsSuccessful) throw new ApplicationException($"Reset session failed. Request not success. {response.StatusCode}");
            ParsePageData(response);
        }

        private string GetInnCardId(string inn)
        {
            var response = _restClient.Post(DebtorsSearchRequest()
                   .AddHeader("Referer", _referer)
                   .AddForm(_viewState, _viewStateGenerator, _previewPage, inn));
            ParsePageData(response);
            var match = _cardIdRegex.Match(response.Content);
            if (!match.Success) return null;
            var id = match.Groups["id"].Value;
            return id;
        }

        private string GetDocId(string cardId)
        {
            var response = _restClient.Get(PrivatePersonCardRequest()
                   .AddHeader("Referer", _referer)
                   .AddQueryParameter("ID", cardId)
                   );
            ParsePageData(response);
            return GetDocIdByAgilityPack(response.Content);
            //return GetDocIdByRegex(response.Content);
        }

        private string GetDocIdByRegex(string content)
        {
            var match = _docIdRegex1.Match(content);
            if (!match.Success) return null;
            var id = match.Groups["docId"].Value;
            return id;
        }

        private string GetDocIdByAgilityPack(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes("//a[text()[contains(.,'Сообщение о судебном акте')]]");
            if (nodes == null) return null;

            // Грубое нарушение, но мне в падлу так много рефакторить из-за одной строки
            _actsCount = nodes.Count;
            ////////////////////////////////////////////////////////////////////////////


            HtmlNode? node = null;

            if(nodes==null) return null;

            if(nodes.Count == 1)
                node = nodes[0];
            if (nodes.Count > 1)
            {
                node = nodes.FirstOrDefault(x => x.InnerText?.Clear().Trim(' ') == "Сообщение о судебном акте");
                if (node == null)
                    node = nodes.LastOrDefault();
            }
            if (node == null) return null;
            var href = node.GetAttributeValue("href", "");
            if (href == null) return null;
            ///MessageWindow.aspx?ID=E0566F6FF86DCD794654703A0F1B5E7B
            string id = href.Split("ID=", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return id;
        }

        private string GetDocHTML(string docId)
        {
            var response = _restClient.Get(MessageWindowRequest()
                      .AddHeader("Referer", _referer)
                      .AddQueryParameter("ID", docId)
                      );
            ParsePageData(response);

            //var match = _docTextRegex.Match(response.Content);
            //if (!match.Success) return null;
            //var text = match.Groups["text"].Value;
            //text = _htmlTags.Replace(text, "");
            return response.Content;
        }

        private void CreateSession()
        {
            GetBankrotCookie();
            ActivateSession();
            SwitchToPersons();
        }

        private void GetBankrotCookie()
        {
            var response = _restClient.Get(DebtorsSearchRequest().AddQueryParameter("attempt", 1));
            if (!response.IsSuccessful) throw new ApplicationException($"Getting bankrot cookie failed. Request /?attempt=1 not success. {response.StatusCode}");
            var codes = _protectCodeRegex.Matches(response.Content).Select(x => x.Groups["code"].Value).ToList();
            var bankrotcookie = Cryptography.Decrypt(codes[2], codes[0], codes[1]);
            _restClient.AddCookie("bankrotcookie", bankrotcookie, "/", "old.bankrot.fedresurs.ru");
        }

        private void ActivateSession()
        {
            var response = _restClient.Get(DebtorsSearchRequest());
            if (!response.IsSuccessful)
                throw new ApplicationException($"Activate session failed. Request /?attempt=2 not success. {response.StatusCode}");

            ParsePageData(response);
        }

        private void SwitchToPersons()
        {
            var response = _restClient.Post(DebtorsSearchRequest()
                .AddHeader("Referer", _referer)
                .AddForm(_viewState, _viewStateGenerator, _previewPage, ""));
            var cookies = response.Cookies.ToList();

            if (!response.IsSuccessful || response.ResponseUri.ToString().Contains("Error.aspx"))
                throw new ApplicationException($"Switch to persons failed. Request not success. {response.StatusCode}");

            ParsePageData(response);
        }

        private RestRequest DebtorsSearchRequest()
        {
            return new RestRequest("/DebtorsSearch.aspx");
        }

        private RestRequest PrivatePersonCardRequest()
        {
            return new RestRequest("/PrivatePersonCard.aspx");
        }

        private RestRequest MessageWindowRequest()
        {
            return new RestRequest("/MessageWindow.aspx");
        }

        private void ParsePageData(RestResponse response)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response.Content);

            _viewState = doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATE']").GetAttributeValue("value", "");
            _formUri = doc.DocumentNode.SelectSingleNode("//*[@id='aspnetForm']").GetAttributeValue("action", "").TrimStart("./".ToCharArray()); 
             _viewStateGenerator = doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR']").GetAttributeValue("value", "");
            _previewPage = doc.DocumentNode.SelectSingleNode("//input[@id='__PREVIOUSPAGE']")?.GetAttributeValue("value", "");
            _referer = response.ResponseUri.ToString();

            //_formUri = _formUriRegex.Match(response.Content).Groups["uri"].Value;
            //_viewState = _viewStateRegex.Match(response.Content).Groups["data"].Value;
            //_viewStateGenerator = _viewStateGeneratorRegex.Match(response.Content).Groups["data"].Value;
            //_previewPage = _previewPageRegex.Match(response.Content).Groups["data"].Value;
        }

    }
}