using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImportAndValidationTool.Import;

namespace ImportAndValidationTool.Helpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient Client = new HttpClient();

        public static HttpContent CreateProductImportHttpContent(IEnumerable<CatalogContentExternalImportModel> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        public static HttpContent CreatePriceImportHttpContent(IEnumerable<SkuPrice> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        public static HttpContent CreateStockImportHttpContent(IEnumerable<StockQuantity> batch)
        {
            const string mediaType = "application/json";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(batch));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(mediaType);
            return content;
        }

        public static void SetupHeaders()
        {
            const string mediaType = "application/json";
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ConfigurationManager.AppSettings["api-key"]);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }

        public static async Task<HttpResponseMessage> ExecuteRequest(HttpContent content, string url, string endpoint)
        {
            SetupHeaders();
            return await Client.PostAsync(url + endpoint, content);
        }
    }
}
