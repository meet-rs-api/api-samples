using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vivani.Meet.Samples.Api
{
    class Program
    {
        private static string apiHost = "https://api.meet.rs";
        private static string apiKey = "PUT YOUR API KEY HERE";
        private static string userSecret = "PUT YOUR API SECRET HERE";

        static void Main(string[] args)
        {
            Console.WriteLine("\n[ApiSample]::START-" + DateTime.Now);
            ProcessAsync(args).GetAwaiter().GetResult();
            Console.WriteLine("\n[ApiSample]::END-" + DateTime.Now);

            Console.WriteLine("\n[ApiSample]::Press any key to finish sample");
            Console.ReadKey();

        }
        private static async Task ProcessAsync(string[] args)
        {
            var tokenInfo = await GetTokenAsync(apiKey, userSecret);

            var quickMeeting = await GetQuickMeeting(tokenInfo.access_token);

            Console.WriteLine("You can try out your new quick meeting at: \n\n" + quickMeeting.JoinUrl);
        }


        private static async Task<MeetingInfo> GetQuickMeeting(string token)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(apiHost + "/v1/meetings");
                var message = new HttpRequestMessage(HttpMethod.Post, uri);
                message.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                var payload = JsonConvert.SerializeObject(new { });
                message.Content = new StringContent(payload);
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await client.SendAsync(message);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MeetingInfo>(content);
            }
        }

        private static async Task<TokenInfo> GetTokenAsync(string key, string secret)
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(apiHost + "/v1/token");
                var message = new HttpRequestMessage(HttpMethod.Post, uri);
                var payload = JsonConvert.SerializeObject(new
                {
                    grant_type = "client_credentials",
                    client_key = key,
                    client_secret = secret,
                });
                message.Content = new StringContent(payload);
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await client.SendAsync(message);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TokenInfo>(content);
            }
        }
    }

    class TokenInfo
    {
        public string access_token { get; set; }

        public long expires_at { get; set; }
    }

    class MeetingInfo
    {
        public string JoinUrl { get; set; }
    }
}
