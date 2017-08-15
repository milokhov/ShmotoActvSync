using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        //curl 'https://motoactv.com/session/login.json' -H 'Origin: https://motoactv.com' -H 'Accept-Encoding: gzip, deflate, br' -H 'Accept-Language: en-US,en;q=0.8,ru;q=0.6,uk;q=0.4,es;q=0.2' 
        // -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36' -H 'Content-Type: application/x-www-form-urlencoded; charset=UTF-8' 
        //-H 'Accept: application/json, text/javascript, */*; q=0.01' 
        //-H 'Referer: https://motoactv.com/' -H 'X-Requested-With: XMLHttpRequest' -H 'Connection: keep-alive' -H 'DNT: 1' --data 'screen_name=milokhov%40live.com&password=actima2005&remember_me=0' --compressed

        //HTTP/1.1 200 OK
        //Server: Apache-Coyote/1.1
        //Set-Cookie: JSESSIONID=0C950A5054973836E8E73ADB9C15FBD6; Path=/; Secure; HttpOnly
        //ETag: "0f90a673825b7c029bd6059fee4785e8d"
        //Content-Type: application/json;charset=utf-8
        //Content-Length: 44
        //Date: Fri, 11 Aug 2017 14:50:48 GMT


        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Origin", "https://motoactv.com");
            client.DefaultRequestHeaders.Add("Referer", "https://motoactv.com");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var dict = new Dictionary<string, string> {
                {"screen_name", "milokhov@live.com" },
                {"password", "actima2005" },
                {"remember_me", "0" },
            };
            var response = await client.PostAsync("https://motoactv.com/session/login.json", new FormUrlEncodedContent(dict));
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status code:{response.StatusCode}");
            Console.WriteLine($"Cookie: {cookies.GetCookies(new Uri("https://motoactv.com"))["JSESSIONID"].Value}");
            Console.WriteLine($"content: {content}");
        }
    }
}
