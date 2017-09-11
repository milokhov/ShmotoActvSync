using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShmotoActvSync.Services
{
    public class StravaService : IStravaService
    {
        private readonly IDbService dbService;
        private string token;

        public StravaService(IDbService dbService)
        {
            this.dbService = dbService;
            this.token = null;
        }

        private string Token => token ?? (token = dbService.GetCurrentUser().StravaToken);

        public async Task<UploadActivityResult> UploadActivity(Stream stream, string fileName, string activityId) // .tcx will be added to activity by strava
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", fileName);
            var response = await client.PostAsync($"https://www.strava.com/api/v3/uploads?data_type=tcx&private=1&external_id={activityId}", content);
            response.EnsureSuccessStatusCode();
            var resultStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(resultStr, new { id = 0L, error = "" });
            return new UploadActivityResult { Id = result.id, Error = result.error };
        }

        public async Task<IEnumerable<AthleteActivity>> GetAthleteActivities()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var response = await client.GetStringAsync($"https://www.strava.com/api/v3/athlete/activities");
            return JsonConvert.DeserializeObject<AthleteActivity[]>(response);
        }

    }

    public class AthleteActivity
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
    }

    public class UploadActivityResult
    {
        public long Id { get; set; }
        public string Error { get; set; }
    }
}
