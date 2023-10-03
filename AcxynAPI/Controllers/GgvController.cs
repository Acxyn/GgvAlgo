using AcxynAPI.Common;
using AcxynAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AcxynAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GgvController : ControllerBase
    {
        private readonly ILogger<GgvController> _logger;
        private readonly IConfiguration _configuration;

        public GgvController(ILogger<GgvController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task CalculateGGVAsync()
        {
            string token = await AdminLoginAsync();
            //Get all game list from APIs and update to database
            await GetGameList(token);
            //Get all game data
            List<Game> gameList = GetAllGameData();

            foreach(Game game in gameList)
            {
                game.GGV = GgvAlgorithm(game, gameList);
                await UpdateGGVAsync(game, token);
            }

            var y = 0;
        }
        // Gross Game Value algorithm calculation
        // Consist on 4 major parameter: 
        // 1. Gross Merchandise Value (GMV)
        // 2. Crypto Valuation Framework (CVF)
        // 3. Developer Metrics (DM)
        // 4. Game Metrics (GM)
        // GGV score is from 0-1000

        [HttpPost("GGVDemo")]
        public async Task UpdateGGVDemo()
        {
            string token = await AdminLoginAsync();
            Game game = new Game();
            game.GameId = 48;
            int ggv = await GetGameGGV(token, game.GameId);
            Random rnd = new Random();
            game.GGV = ggv + rnd.Next(1, 10);
            await UpdateGGVAsync(game, token);
        }

        [HttpPost("GGVDemoReset")]
        public async Task ResetGGVDemo()
        {
            string token = await AdminLoginAsync();
            Game game = new Game();
            game.GameId = 48;
            game.GGV = 500;
            await UpdateGGVAsync(game, token);
        }

        private int GgvAlgorithm(Game game, List<Game> gameList)
        {
            int ggv = 0;
            //1. Gross Merchandise Value (GMV)
            // BS (Bucket Size) = Total In App Purchase / No. of Transaction
            // BGMV (Base GMV score) = 500
            // GMV = BGMV * (BS / Total Average BS * 100) / 100
            // GMV max cap at 1000

            //Eg. set game = GameA
            //string game = "GameA";
            decimal bs = game.BucketSize;
            decimal averageBs = gameList.Sum(a => a.BucketSize) / gameList.Count();

            int bgmv = 500;
            int gmv = (int)(bgmv * (bs / averageBs * 100) / 100);
            if (gmv > 1000)
                gmv = 1000;

            //2. Crypto Valuation Framework (CVF)
            decimal nv = game.NetworkValue;
            decimal trxVol = game.TransactionVolume;
            decimal cvf = nv / trxVol;

            //3. Developer Metrics (DM)
            // Integrated 10 function consider scoring the max 
            int integratedCount = game.IntegratedFunction;
            int integrationScore = 100;
            if (integratedCount > 10)
                integratedCount = 10;
            int dm = integratedCount * integrationScore;

            //4. Game Metrics (GM)
            decimal prWeight = 0.25m;
            decimal pcWeight = 0.25m;
            decimal ptWeight = 0.3m;
            decimal adrWeight = 0.1m;
            decimal rpuWeight = 0.1m;

            decimal avgPlyrRetention = game.AvgPlayerRetention;
            decimal avgPlyrCount = game.AvgPlayerCount;
            decimal avgPlyTime = game.AvgPlayTime;
            decimal avgAdRev = game.AvgAdRev;
            decimal avgRPU = game.AvgRPU;

            int bgm = 500;
            int gm = ((int)(bgm * ((avgPlyrRetention * prWeight) + (avgPlyrCount * pcWeight) + (avgPlyTime * ptWeight) + (avgAdRev * adrWeight) + (avgRPU * rpuWeight)) / 100));

            //GGV computation
            // GGV = ((GMV * GMV Weight) + (CVF * CVF Weight) + (DM * DM Weight) + (GM * GM Weight))
            decimal gmvWeight = 0.5m;
            decimal cvfWeight = 0.2m;
            decimal dmWeight = 0.1m;
            decimal gmWeight = 0.2m;

            ggv = ((int)(gmv * gmvWeight) + (int)(cvf * cvfWeight) + (int)(dm * dmWeight) + (int)(gm * gmWeight));
            return ggv;
        }

        private List<Game> GetAllGameData()
        {
            List<Game> gameList = new List<Game>();

            try
            {
                string sql = @"SELECT * FROM tbl_ggv_data ";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        Game data = new Game();
                        data.GameId = Convert.ToInt32(item["game_id"]);
                        data.BucketSize = Convert.ToDecimal(item["bucket_size"]);
                        data.NetworkValue = Convert.ToDecimal(item["network_value"]);
                        data.TransactionVolume = Convert.ToDecimal(item["transaction_volume"]);
                        data.IntegratedFunction = Convert.ToInt32(item["integrated_function"]);
                        data.AvgPlayerRetention = Convert.ToInt32(item["avg_player_retention"]);
                        data.AvgPlayerCount = Convert.ToInt32(item["avg_player_count"]);
                        data.AvgPlayTime = Convert.ToInt32(item["avg_play_time"]);
                        data.AvgAdRev = Convert.ToInt32(item["avg_ad_rev"]);
                        data.AvgRPU = Convert.ToInt32(item["avg_rpu"]);

                        gameList.Add(data);
                    }
                }
                return gameList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<string> AdminLoginAsync()
        {
            string token = string.Empty;
            // Create an instance of HttpClient
            using (var httpClient = new HttpClient())
            {
                // Set the Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "your_bearer_token");


                LoginReq req = new LoginReq();
                req.email = "dave@acxyn.xyz";
                req.password = "Pass@123";
                // Prepare the JSON payload
                //var jsonPayload = "{\"email\":\"dave@acxyn.xyz\",\"password\":\"Pass@123\"}";
                var jsonPayload = JsonSerializer.Serialize(req);

                // Create the HTTP content with JSON payload
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await httpClient.PostAsync("https://dev-api.acxyn.io/super-admin/api/login", httpContent);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Get the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var resultJson = JsonSerializer.Deserialize<LoginResp>(responseContent);
                    token = resultJson.token;
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }
            }

            return token;
        }

        private async Task<string> UpdateGGVAsync(Game game, string token)
        {
            // Create an instance of HttpClient
            using (var httpClient = new HttpClient())
            {
                // Set the Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                UpdateGGVReq req = new UpdateGGVReq();
                req.gmv = 50;
                req.pe_valuation = 20;
                req.player_metric = 20;
                req.dev_activity = 10;
                req.ggv = game.GGV;
                // Prepare the JSON payload
                //var jsonPayload = "{\"email\":\"dave@acxyn.xyz\",\"password\":\"Pass@123\"}";
                var jsonPayload = JsonSerializer.Serialize(req);
                // Create the HTTP content with JSON payload
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var url = "https://dev-api.acxyn.io/super-admin/api/game/analytics/";
                url += game.GameId;
                // Send the POST request
                var response = await httpClient.PatchAsync(url, httpContent);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Get the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }
            }

            return "";
        }

        private async Task<string> GetGameList(string token)
        {
            // Create an instance of HttpClient
            using (var httpClient = new HttpClient())
            {
                // Set the Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Prepare the JSON payload
                //var jsonPayload = "{\"email\":\"dave@acxyn.xyz\",\"password\":\"Pass@123\"}";
                //var jsonPayload = JsonSerializer.Serialize(req);
                // Create the HTTP content with JSON payload
                var url = "https://dev-api.acxyn.io/super-admin/api/game";
                // Send the POST request
                var response = await httpClient.GetAsync(url);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Get the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var resultJson = JsonSerializer.Deserialize<GameListResp>(responseContent);
                    UpsertGameData(resultJson);
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }
            }

            return "";
        }

        private async Task<int> GetGameGGV(string token, int gameId)
        {
            // Create an instance of HttpClient
            using (var httpClient = new HttpClient())
            {
                // Set the Bearer token
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Prepare the JSON payload
                //var jsonPayload = "{\"email\":\"dave@acxyn.xyz\",\"password\":\"Pass@123\"}";
                //var jsonPayload = JsonSerializer.Serialize(req);
                // Create the HTTP content with JSON payload
                var url = "https://dev-api.acxyn.io/super-admin/api/game/analytics/" + gameId;
                // Send the POST request
                var response = await httpClient.GetAsync(url);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Get the response content
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    var resultJson = JsonSerializer.Deserialize<GameResp>(responseContent);
                    return resultJson.data.ggv;
                }
                else
                {
                    Console.WriteLine("Request failed with status code: " + response.StatusCode);
                }
            }

            return 0;
        }

        private bool UpsertGameData(GameListResp gameList)
        {
            var existingGameList = GetAllGameData();
            var sql = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
         
            for (var i = 0; i < gameList.data.Count; i++)
            {
                if (existingGameList.Any(a => a.GameId == gameList.data[i].id))
                {
                    //update
                    sql += $"UPDATE tbl_ggv_data SET bucket_size = @bucket_size_{i}, transaction_volume = @transaction_volume_{i}, integrated_function = @integrated_function_{i}, avg_player_retention = @avg_player_retention_{i}, avg_player_count =  @avg_player_count_{i}, avg_play_time =  @avg_play_time_{i}, avg_ad_rev = @avg_ad_rev_{i}, avg_rpu = @avg_rpu_{i}, network_value= @network_value_{i} WHERE game_id = @game_id_{i}; ";
                    parameters.Add($"@bucket_size_{i}", gameList.data[i].bucket_size);
                    parameters.Add($"@transaction_volume_{i}", gameList.data[i].transaction_volume);
                    parameters.Add($"@integrated_function_{i}", gameList.data[i].integrated_function);
                    parameters.Add($"@avg_player_retention_{i}", gameList.data[i].avg_player_retention);
                    parameters.Add($"@avg_player_count_{i}", gameList.data[i].avg_player_count);
                    parameters.Add($"@avg_play_time_{i}", gameList.data[i].avg_play_time);
                    parameters.Add($"@avg_ad_rev_{i}", gameList.data[i].avg_ad_rev);
                    parameters.Add($"@avg_rpu_{i}", gameList.data[i].avg_rpu);
                    parameters.Add($"@network_value_{i}", gameList.data[i].network_value);
                    parameters.Add($"@game_id_{i}", gameList.data[i].id);
                }
                else
                {
                    //insert
                    sql += $"INSERT INTO tbl_ggv_data (game_id, bucket_size, transaction_volume, integrated_function, avg_player_retention, avg_player_count, avg_play_time, avg_ad_rev, avg_rpu, network_value) VALUES (@game_id_{i}, @bucket_size_{i}, @transaction_volume_{i}, @integrated_function_{i}, @avg_player_retention_{i}, @avg_player_count_{i}, @avg_play_time_{i}, @avg_ad_rev_{i}, @avg_rpu_{i}, @network_value_{i}); ";
                    parameters.Add($"@bucket_size_{i}", gameList.data[i].bucket_size);
                    parameters.Add($"@transaction_volume_{i}", gameList.data[i].transaction_volume);
                    parameters.Add($"@integrated_function_{i}", gameList.data[i].integrated_function);
                    parameters.Add($"@avg_player_retention_{i}", gameList.data[i].avg_player_retention);
                    parameters.Add($"@avg_player_count_{i}", gameList.data[i].avg_player_count);
                    parameters.Add($"@avg_play_time_{i}", gameList.data[i].avg_play_time);
                    parameters.Add($"@avg_ad_rev_{i}", gameList.data[i].avg_ad_rev);
                    parameters.Add($"@avg_rpu_{i}", gameList.data[i].avg_rpu);
                    parameters.Add($"@network_value_{i}", gameList.data[i].network_value);
                    parameters.Add($"@game_id_{i}", gameList.data[i].id);
                }
            }                
            
            DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
            return true;
        }
    }
}
