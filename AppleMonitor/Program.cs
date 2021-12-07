using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppleMonitor.Model;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace AppleMonitor
{
    internal class Program
    {
        private const string fsMSgUrl = "https://sctapi.ftqq.com/SCT3076Ts8vGSupcrKOsPLbddXOHmWbG.send";

        //public static readonly WebProxy GetTunnelProxy = new WebProxy()
        //{
        //    Address = new Uri("http://127.0.0.1:8866")
        //};

        static void Main(string[] args)
        {
            var monitorList = new List<string>()
            {
                "iPhone 13 Pro 256GB 银色"
            };

            var url = "https://www.apple.com.cn/shop/pickup-message-recommendations?mt=compact&location=江苏 南京 玄武区&product=MLTC3CH/A";
            while (true)
            {
                try
                {
                    var client = new RestClient(url);
                    client.UseNewtonsoftJson();
                    //client.Proxy = GetTunnelProxy;
                    var request = new RestRequest(Method.GET);
                    var response = client.Execute<QueryInfoModel>(request);
                    var storelist = new List<string>()
                    {
                        "南京艾尚天地", 
                        "虹悦城", 
                        "玄武湖"
                    };
                    foreach (string i in storelist) {
                        var storeInfo = response.Data.Body.PickupMessage.Stores.FirstOrDefault(x => x.StoreName == i);

                        if (storeInfo == null) continue;

                        var productList =
                            storeInfo.PartsAvailability.Where(x => monitorList.Contains(x.Value.StorePickupProductTitle))
                                .Select(x => x.Value).ToList();


                        if (productList.Any())
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"{DateTime.Now}");
                            sb.AppendLine("---------------------");
                            foreach (var product in productList)
                            {
                                sb.AppendLine($"- {product.StorePickupProductTitle}:{product.StorePickupQuote}");
                            }
                            Console.WriteLine(sb.ToString());
                            Send(sb.ToString());
                        }
                        else
                        {
                            Console.WriteLine($"{DateTime.Now}\t{i}:暂时无货");
                        }
                    }
                    Thread.Sleep(1000 * 60);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            }
        }


        public static void Send(string msg)
        {
            try
            {

                Debug.WriteLine($"{DateTime.Now}\n{msg}");

                var client = new RestClient(fsMSgUrl);
                var request = new RestRequest(Method.POST);
                //client.Proxy = GetTunnelProxy;
                request.AddParameter("title","Apple 监控提醒");
                request.AddParameter("desp", msg);
                var response = client.Execute(request);
                Console.WriteLine($"Server酱消息:{response.Content}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
