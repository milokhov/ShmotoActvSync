using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShmotoActvSync.Services
{
    public class MotoActvService : IMotoActvService
    {
        const string motoUrl = "https://motoactv.com";

        public async Task<(bool, string)> VerifyPassword(string username, string password)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Origin", motoUrl);
            client.DefaultRequestHeaders.Add("Referer", motoUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var dict = new Dictionary<string, string> {
                {"screen_name", username },
                {"password", password },
                {"remember_me", "0" },
            };
            var response = await client.PostAsync($"{motoUrl}/session/login.json", new FormUrlEncodedContent(dict));
            var content = await response.Content.ReadAsStringAsync();
            var contentObj = JsonConvert.DeserializeAnonymousType(content, new { code = 0 });
            if (!response.IsSuccessStatusCode || contentObj.code != 1)
                return (false, $"Authentication failed, status code: {response.StatusCode}, Content: {content}");
            else
            {
                if (cookies.GetCookies(new Uri(motoUrl))["JSESSIONID"] == null)
                {
                    return (false, $"Authentication failed (no code provided), Content: {content}");
                }
                return (true, null);
            }
        }
    }
}
