using Anticaptcha_example.Api;
using RestSharp;
using Shared;
using System.Text;
using System.Text.RegularExpressions;

namespace Kommersant
{
    public class KommersantParser<TData> : IInnParser<ParsedDataBase> where TData : IParsedData
    {
        private const string BaseUrl = "https://bankruptcy.kommersant.ru";
        private const string FullUrl = "https://bankruptcy.kommersant.ru/search/poisk_soobshcheniya_o_bankrotstve";
        private readonly string _antiCaptchaKey;
        private readonly RestClient _restClient;
        private readonly Regex _dataSiteKeyRegex = new Regex("data-sitekey=\"(?<key>.+?)\"");
        private readonly Regex _hashRegex = new Regex("{\"hash\":\"(?<code>.+?)\"}");
        private readonly Regex _messageRegex = new Regex("<div class=\"page-content-company\">(?<html>.+?)<div class=\"show-all-mess bankr\">", RegexOptions.Singleline);

        private string _dataSiteKey;


        public KommersantParser(string antiCaptchaKey)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _antiCaptchaKey = antiCaptchaKey;
            _restClient = new RestClient(KommersantParser<TData>.BaseUrl);
            _restClient.AddDefaultHeader("Accept-Language", "ru-RU,ru;q=0.9");
            _restClient.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36");
            CreateSession();
        }

        public ParsedDataBase Parse(string inn)
        {
            try
            {
                string recaptchaResponse = GetRecaptchaResponse();
                string hash = GetInnHash(inn, recaptchaResponse);
                var texts = GetResultHtml(hash);
                return new KommersantParsedData(inn, hash, texts);
            }catch(Exception ex)
            {
                return new KommersantParsedData(inn, ex);
            }
            //ParseRecaptchaData(response);
        }

        private void CreateSession()
        {
            var response = _restClient.Get(new RestRequest("/search/index.php"));
            if (!response.IsSuccessful) throw new Exception("Create session failed. Request not success.");
            ParseRecaptchaData(response);
        }

        private void ParseRecaptchaData(RestResponse response)
        {
            _dataSiteKey = _dataSiteKeyRegex.Match(response.Content).Groups["key"].Value;
            return;
        }

        private string GetResultHtml(string hash)
        {
            var response = _restClient.Get(new RestRequest($"/search/poisk_soobshcheniya_o_bankrotstve/{hash}/"));

            if (!response.IsSuccessful)
                throw new Exception($"Error on get text of {hash}. Request not success.");

            ParseRecaptchaData(response);

            var win1251Content = Encoding.GetEncoding(1251).GetString( response.RawBytes);

            return win1251Content;
        }

        private string GetRecaptchaResponse()
        {
            var api = new RecaptchaV2Proxyless
            {
                ClientKey = _antiCaptchaKey,
                WebsiteUrl = new Uri(FullUrl),
                WebsiteKey = _dataSiteKey
            };

            if (!api.CreateTask())
                throw new Exception($"Create captcha task failed. API v2 send failed. {api.ErrorMessage}");
            else if (!api.WaitForResult())
                throw new Exception($"Could not solve the captcha.");
            else
                return api.GetTaskSolution().GRecaptchaResponse;
        }

        private string GetInnHash(string inn, string recaptchaResponse)
        {
            var response = _restClient.Post(new RestRequest("/search/poisk_soobshcheniya_o_bankrotstve/?action=Hash")
                .AddParameter("rekvsbk", "on", true)
                .AddParameter("query", inn, true)
                .AddParameter("g-recaptcha-response", recaptchaResponse, true)
                .AddParameter("undefined", "undefined", true)
                );

            if (!response.IsSuccessful)
                throw new Exception($"Error on get hash of {inn}. Request not success.");

            return _hashRegex.Match(response.Content).Groups["code"].Value;
        }
    }
}