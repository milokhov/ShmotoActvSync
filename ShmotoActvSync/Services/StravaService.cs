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


        public async Task Test()
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);


            var response = await client.GetAsync($"https://www.strava.com/api/v3/athlete");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

        }

        public async Task UploadActivity(Stream stream, string fileName, string activityId) // .tcx will be added to activity by strava
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", fileName);
            var response = await client.PostAsync($"https://www.strava.com/api/v3/uploads?data_type=tcx&private=1&external_id={activityId}", content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
        }
    }
}
