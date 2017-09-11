using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text;

namespace ShmotoActvSync.Services
{
    public class MotoActvService : IMotoActvService
    {
        const string motoUrl = "https://motoactv.com";
        private readonly IMotoActvCredentialsProvider credentialsProvider;

        public MotoActvService(IMotoActvCredentialsProvider credentialsProvider)
        {
            this.credentialsProvider = credentialsProvider;
        }

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

        private async Task<HttpClient> PrepareSession()
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Origin", motoUrl);
            client.DefaultRequestHeaders.Add("Referer", motoUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            (var username, var password) = credentialsProvider.GetCredentials();
            var dict = new Dictionary<string, string> {
                {"screen_name", username },
                {"password", password },
                {"remember_me", "0" },
            };
            var response = await client.PostAsync($"{motoUrl}/session/login.json", new FormUrlEncodedContent(dict));
            var content = await response.Content.ReadAsStringAsync();
            var contentObj = JsonConvert.DeserializeAnonymousType(content, new { code = 0 });
            if (!response.IsSuccessStatusCode || contentObj.code != 1)
                throw new Exception($"Authentication failed, status code: {response.StatusCode}, Content: {content}");
            else
            {
                if (cookies.GetCookies(new Uri(motoUrl))["JSESSIONID"] == null)
                {
                    throw new Exception("No Session ID found in reply");
                }
                return client;
            }
        }

        public async Task<MotoActvWorkouts> GetWorkouts(DateTime startDate, DateTime endDate)
        {
            var client = await PrepareSession();
            var response = await client.PostAsync($"{motoUrl}/data/pastAllWorkouts/2.json", new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "getall", "yep" },
                    { "startDate", new DateTimeOffset(startDate).ToUnixTimeMilliseconds().ToString() },
                    { "endDate", new DateTimeOffset(endDate).ToUnixTimeMilliseconds().ToString() },
                }
                ));
            var responseObj = JsonConvert.DeserializeObject<MotoActvWorkouts>(await response.Content.ReadAsStringAsync());
            return responseObj;
        }

        public async Task<MotoActvWorkouts> GetRecentWorkouts()
        {
            return await GetWorkouts(DateTime.Now.AddDays(-30), DateTime.Now.AddYears(1));
        }

        public async Task<Stream> RetrieveWorkout(string workoutId)
        {
            var client = await PrepareSession();
            var response = await client.PostAsync($"{motoUrl}/export/exportSingleWorkoutData.json", new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "activityId", workoutId },
                    { "formats[]", "TCX" },
                }
                ));
            response.EnsureSuccessStatusCode();
            var responseObj = JsonConvert.DeserializeObject<MotoActvExport>(await response.Content.ReadAsStringAsync());

            using (var exportFileStream = await client.GetStreamAsync($"{motoUrl}/export/downloadExport?exportID=" + responseObj.ExportID))
            {
                using (ZipInputStream zipInputStream = new ZipInputStream(exportFileStream))
                {
                    ZipEntry zipEntry = zipInputStream.GetNextEntry();
                    if (zipEntry != null)
                    {
                        var entryFileName = zipEntry.Name;
                        var stream = new MemoryStream();
                        zipInputStream.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        return stream;
                        //using (var reader = new StreamReader(zipInputStream, Encoding.UTF8))
                        //{
                        //    string value = reader.ReadToEnd();
                        //    // Do something with the value
                        //}
                    }
                    else throw new Exception("Invalid export stream");
                }
            }
        }
    }

    public class MotoActvExport
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("exportId")]
        public string ExportID { get; set; }
        [JsonProperty("workoutsExported")]
        public int WorkoutsExported { get; set; }
    }

    public class MotoActvWorkouts
    {
        [JsonProperty("pastWorkouts")]
        public MotoActvWorkout[] Workouts { get; set; }
    }

    public class MotoActvWorkout
    {
        [JsonProperty("startTime")]
        public long StartTimeUnix { get; set; }
        public DateTime StartTime => DateTimeOffset.FromUnixTimeMilliseconds(StartTimeUnix).LocalDateTime;
        [JsonProperty("endTime")]
        public long EndTimeUnix { get; set; }
        public DateTime EndTime => DateTimeOffset.FromUnixTimeMilliseconds(EndTimeUnix).LocalDateTime;
        [JsonProperty("activity")]
        public int ActivityType { get; set; } // 1 - running, 4 - cycling
        [JsonProperty("workoutActivityId")]
        public string WorkoutActivityId { get; set; }
    }
}
