using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Collections;


namespace Misc
{
    namespace Projects
    {
        public class StockInfo
        {
            public String open { get; set; }
            public String close { get; set; }
            public String date { get; set; }
            public String high { get; set; }
            public String low { get; set; }
            public String volume { get; set; }
            public String change { get; set; }
            public String changePercent { get; set; }
        }

        //public class Response
        //{
        //    List<StockInfo> StockInfoList { get; set; }
        //}


        public class InfoCollector
        {
            static HttpClient client;

            public void ClientSetup()
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
                return await client.GetAsync(WebUtility.UrlEncode(args));
            }

            public async Task<String> GetStringFromResponse(HttpResponseMessage response)
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else { throw new HttpRequestException("Bad response from request"); }
            }

            //public async Task<Stream> GetStreamFromResponse(HttpResponseMessage response)
            //{
            //    if (response.IsSuccessStatusCode)
            //    {
            //        return await response.Content.ReadAsStreamAsync();
            //    }
            //    else { throw new HttpRequestException("Bad response from request"); }
            //}

        }

        class GetData
        {
            static void Main(string[] args)
            {
                InfoCollector collector = new InfoCollector();
                collector.ClientSetup();
                
                HttpResponseMessage response = collector.GetResponseFromRequest("stock/clf/chart/5y").Result;

                String JsonString = collector.GetStringFromResponse(response).Result;

                JArray jsonObjectArray = JArray.Parse(JsonString);

                //https://stackoverflow.com/questions/15726197/parsing-a-json-array-using-json-net
                foreach(JObject jsonObject in jsonObjectArray.Children<JObject>())
                {
                    foreach (JProperty jsonProperty in jsonObject.Properties())
                    {
                        String name = jsonProperty.Name;
                        String value = (String)jsonProperty.Value;
                    }
                }

                ////https://stackoverflow.com/questions/12676746/parse-json-string-in-c-sharp
                //foreach (JObject jsonObject in jsonObjectArray)
                //{
                //    foreach (KeyValuePair<String, JToken> obj in jsonObject)
                //    {
                //        //var Key = obj.Key;
                //        var close = (String)obj.Value["close"];
                //    }
                //}

                //List<StockInfo> si = JsonConvert.DeserializeObject<List<StockInfo>>(JsonString);

                IEnumerable<StockInfo> si2 = JsonConvert.DeserializeObject<IEnumerable<StockInfo>>(JsonString).Reverse();



            }
        }
    }
}
