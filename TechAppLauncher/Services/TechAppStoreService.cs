using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TechAppLauncher.Helpers;
using TechAppLauncher.Models;
using System.Xml;
using System.Xml.Linq;

namespace TechAppLauncher.Services
{
    public class TechAppStoreService
    {
        private static HttpClient s_httpClient;

        public static async Task<IList<Models.App>> GetAllAsync()
        {
            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential("zulhisham.tanabdulla", "AZ@ZThoe03081972", "petronas")
            };

            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            string url = @"https://techupstream.petronas.com/tastore/_api/lists/getbytitle('AppList')/items?$orderby=AppGroup,Title asc";
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //request.Content = new StringContent("", Encoding.UTF8, "application/json");
            

            s_httpClient = new HttpClient(handler);
            s_httpClient.DefaultRequestHeaders.Accept.Clear();
            s_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await s_httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            //var content = await response.Content.ReadFromJsonAsync(typeof(Models.AppDetail));
            var content = await response.Content.ReadAsStringAsync();

            //content = System.IO.File.ReadAllText(@"Data\TestData.txt", Encoding.ASCII);
            //var query = JsonConvert.DeserializeObject<AppDetail>(content);

            Serializer serializer = new Serializer();
            var feeds = serializer.DeserializeFeed(content);


            var results = feeds.entry.Select(s => new Models.App()
            {
                Title = s.content.properties.Title != null ? s.content.properties.Title.Value : "",
                ShortDescription = s.content.properties.ShortDescription != null ? s.content.properties.ShortDescription.Value : "",
                AppLogoUrl = new Models.AppLogoUrl()
                {
                    Url = s.content.properties.AppLogoUrl !=null && s.content.properties.AppLogoUrl.Url != null ? s.content.properties.AppLogoUrl.Url : null,
                }
            }).ToList();

            return results;
        }

        public static async Task<Stream> LoadCoverBitmapAsync(string imgUrl)
        {
            var handler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential("zulhisham.tanabdulla", "AZ@ZThoe03081972", "petronas")
            };

            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };


            s_httpClient = new HttpClient(handler);

            var data = await s_httpClient.GetByteArrayAsync(imgUrl);
            return new MemoryStream(data);
        }
    }
}
