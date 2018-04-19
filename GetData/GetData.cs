using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Misc
{
    namespace Projects
    {

        public class InfoCollector
        {
            static HttpClient client;

            public void Setup()
            {
                client = new HttpClient()
                {
                    BaseAddress = new Uri("https://api.iextrading.com/1.0/"),
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            public async Task<HttpResponseMessage> GetResponseFromRequest(String args)
            {
                return await client.GetAsync(args);
            }

            public async Task<String> GetDataFromResponse(HttpResponseMessage response)
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else { throw new HttpRequestException("Bad response from request"); }
            }

        }

        class GetData
        {
            static void Main(string[] args)
            {
                InfoCollector collector = new InfoCollector();
                collector.Setup();
                HttpResponseMessage r = collector.GetResponseFromRequest("stock/aapl/chart").Result;
                
            }
        }
    }
}
