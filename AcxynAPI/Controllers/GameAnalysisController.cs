using AcxynAPI.Common;
using AcxynAPI.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
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
    public class GameAnalysisController : ControllerBase
    {
        private readonly ILogger<GameAnalysisController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public GameAnalysisController(ILogger<GameAnalysisController> logger, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Please select a file.");
            }

            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                var csvDataList = new List<CsvData>();
                string line;
                int count = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (count != 0)
                    {
                        TextFieldParser parser = new TextFieldParser(new StringReader(line));
                        parser.SetDelimiters(",");
                        //string[] rawFields = parser.ReadFields();

                        //var parts = line.Split(',');
                        var parts = parser.ReadFields();
                        if (parts.Length > 0)
                        {
                            var csvData = new CsvData
                            {
                                GameName = parts[4],
                                AvgUserRating = parts[13],
                                NoOfUserRating = parts[14],
                                DAU = parts[15],
                                Installs = parts[16],
                                Playtime = parts[17],
                                D1Retention = parts[18],
                                D7Retention = parts[19],
                                D30Retention = parts[20],
                                AvgSessionCount = parts[21],
                                AvgSessionLength = parts[22],
                                CurrentVisitScore = parts[23],
                                AllTimePerformanceScore = parts[24]
                            };
                            csvDataList.Add(csvData);
                        }
                    }
                    count++;
                }

                // Process the csvDataList as needed
                return Ok(csvDataList);
            }
        }

        private List<GameDataDemo> GetAllGameData()
        {
            List<GameDataDemo> gameList = new List<GameDataDemo>();

            try
            {
                string sql = @"SELECT * FROM tbl_ggv_data_demo ";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        GameDataDemo data = new GameDataDemo();
                        data.id = Convert.ToInt32(item["id"]);                        
                        data.bucket_size = Convert.ToDecimal(item["bucket_size"]);
                        data.network_value = Convert.ToDecimal(item["network_value"]);
                        data.transaction_volume = Convert.ToDecimal(item["transaction_volume"]);
                        data.integrated_function = Convert.ToInt32(item["integrated_function"]);
                        data.avg_player_retention = Convert.ToInt32(item["avg_player_retention"]);
                        data.avg_player_count = Convert.ToInt32(item["avg_player_count"]);
                        data.avg_play_time = Convert.ToInt32(item["avg_play_time"]);
                        data.avg_ad_rev = Convert.ToInt32(item["avg_ad_rev"]);
                        data.avg_rpu = Convert.ToInt32(item["avg_rpu"]);
                        data.GameName = item["game_name"].ToString();
                        //data.ggv = Convert.ToInt32(item["ggv"]);
                        //data.AvgUserRating = Convert.ToDecimal(item["transaction_volume"]);
                        //data.NoOfUserRating = Convert.ToDecimal(item["transaction_volume"]);
                        //data.DAU = Convert.ToDecimal(item["transaction_volume"]);
                        //data.Installs = Convert.ToDecimal(item["transaction_volume"]);
                        //data.Playtime = Convert.ToDecimal(item["transaction_volume"]);
                        //data.D1Retention = Convert.ToDecimal(item["transaction_volume"]);
                        //data.D7Retention = Convert.ToDecimal(item["transaction_volume"]);
                        //data.D7Retention = Convert.ToDecimal(item["transaction_volume"]);
                        //data.D30Retention = Convert.ToDecimal(item["transaction_volume"]);
                        //data.AvgSessionCount = Convert.ToDecimal(item["transaction_volume"]);
                        //data.AvgSessionLength = Convert.ToDecimal(item["transaction_volume"]);
                        //data.CurrentVisitScore = Convert.ToDecimal(item["transaction_volume"]);
                        //data.AllTimePerformanceScore = Convert.ToDecimal(item["transaction_volume"]);

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

        private bool UpsertGameData(List<GameDataDemo> gameList)
        {
            var existingGameList = GetAllGameData();
            var sql = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            for (var i = 0; i < gameList.Count; i++)
            {
                if (existingGameList.Any(a => a.id == gameList[i].id))
                {
                    //update
                    sql += $"UPDATE tbl_ggv_data SET bucket_size = @bucket_size_{i}, transaction_volume = @transaction_volume_{i}, integrated_function = @integrated_function_{i}, avg_player_retention = @avg_player_retention_{i}, avg_player_count =  @avg_player_count_{i}, avg_play_time =  @avg_play_time_{i}, avg_ad_rev = @avg_ad_rev_{i}, avg_rpu = @avg_rpu_{i}, network_value= @network_value_{i} WHERE game_id = @game_id_{i}; ";
                    parameters.Add($"@bucket_size_{i}", gameList[i].bucket_size);
                    parameters.Add($"@transaction_volume_{i}", gameList[i].transaction_volume);
                    parameters.Add($"@integrated_function_{i}", gameList[i].integrated_function);
                    parameters.Add($"@avg_player_retention_{i}", gameList[i].avg_player_retention);
                    parameters.Add($"@avg_player_count_{i}", gameList[i].avg_player_count);
                    parameters.Add($"@avg_play_time_{i}", gameList[i].avg_play_time);
                    parameters.Add($"@avg_ad_rev_{i}", gameList[i].avg_ad_rev);
                    parameters.Add($"@avg_rpu_{i}", gameList[i].avg_rpu);
                    parameters.Add($"@network_value_{i}", gameList[i].network_value);
                    parameters.Add($"@game_id_{i}", gameList[i].id);
                }
                else
                {
                    //insert
                    sql += $"INSERT INTO tbl_ggv_data (game_id, bucket_size, transaction_volume, integrated_function, avg_player_retention, avg_player_count, avg_play_time, avg_ad_rev, avg_rpu, network_value) VALUES (@game_id_{i}, @bucket_size_{i}, @transaction_volume_{i}, @integrated_function_{i}, @avg_player_retention_{i}, @avg_player_count_{i}, @avg_play_time_{i}, @avg_ad_rev_{i}, @avg_rpu_{i}, @network_value_{i}); ";
                    parameters.Add($"@bucket_size_{i}", gameList[i].bucket_size);
                    parameters.Add($"@transaction_volume_{i}", gameList[i].transaction_volume);
                    parameters.Add($"@integrated_function_{i}", gameList[i].integrated_function);
                    parameters.Add($"@avg_player_retention_{i}", gameList[i].avg_player_retention);
                    parameters.Add($"@avg_player_count_{i}", gameList[i].avg_player_count);
                    parameters.Add($"@avg_play_time_{i}", gameList[i].avg_play_time);
                    parameters.Add($"@avg_ad_rev_{i}", gameList[i].avg_ad_rev);
                    parameters.Add($"@avg_rpu_{i}", gameList[i].avg_rpu);
                    parameters.Add($"@network_value_{i}", gameList[i].network_value);
                    parameters.Add($"@game_id_{i}", gameList[i].id);
                }
            }

            DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
            return true;
        }

    }
}
