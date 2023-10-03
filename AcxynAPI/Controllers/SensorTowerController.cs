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
    public class SensorTowerController : ControllerBase
    {
        private readonly ILogger<SensorTowerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SensorTowerController(ILogger<SensorTowerController> logger, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }


        [HttpGet("SyncGameName")]
        public async Task SyncGameName()
        {
            string token = await SyncGameNameAsync();
        }

        [HttpGet("SyncGameData")]
        public async Task SyncGameData()
        {
            string token = await SyncGameDataAsync();
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
                var csvDataList = new List<SensorTowerCsvData>();
                string line;
                int count = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (count != 0)
                    {
                        TextFieldParser parser = new TextFieldParser(new StringReader(line));
                        parser.SetDelimiters("\t");
                        //string[] rawFields = parser.ReadFields();

                        //var parts = line.Split(',');
                        var parts = parser.ReadFields();
                        if (parts.Length > 0)
                        {
                            var csvData = new SensorTowerCsvData
                            {
                                AppId = parts[4],
                                AppName = parts[5],
                                DataDate = Convert.ToDateTime(parts[9]),
                                Revenue = Convert.ToDecimal(parts[13]),
                                Type = "strategy"                              
                            };
                            csvDataList.Add(csvData);
                        }
                    }
                    count++;
                }

                //Insert
                InsertSireData(csvDataList);

                // Process the csvDataList as needed
                return Ok(csvDataList);
            }
        }

        [HttpGet("GetSirePostData")]
        public IActionResult GetSirePostData()
        {
            List<SirePostData> dataList = new List<SirePostData>();

            try
            {
                //string sql = @"SELECT * FROM tbl_sire_data limit 1000;";
                string sql = @"SELECT * FROM tbl_sire_data a
                                JOIN 
                                (
                                    SELECT DISTINCT app_id 
                                    FROM tbl_sire_data 
                                    WHERE data_date >= '2023-08-01'
                                    ORDER BY revenue DESC LIMIT 50
                                ) b
                                ON a.app_id
                                IN (b.app_id) where a.data_date >= '2022-01-01' and revenue < 100000000;";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        var date = Convert.ToDateTime(item["data_date"]);
                        string[] customer_focus = { item["app_name"].ToString() };
                        string[] sectors = { item["type"].ToString() };

                        SirePostData data = new SirePostData();
                        data.id = item["app_id"].ToString();
                        data.company_name = item["app_name"].ToString();
                        data.customer_focus = customer_focus;
                        data.sectors = sectors;
                        data.revenue = item["revenue"].ToString();
                        data.month = date.Month.ToString();
                        data.year = date.Year.ToString();
                        data.yoy_growth = item["yoy_growth"].ToString();
                        data.next_yoy_growth = item["next_yoy_growth"].ToString();

                        dataList.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return Ok(dataList);
        }

        [HttpGet("CalculateYoY")]
        public IActionResult CalculateYoY()
        {
            List<SensorTowerCsvData> dataList = new List<SensorTowerCsvData>();

            try
            {
                string sql = @"SELECT * FROM tbl_sire_data limit 66000;";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        var date = Convert.ToDateTime(item["data_date"]);

                        SensorTowerCsvData data = new SensorTowerCsvData();
                        data.Id = Convert.ToInt32(item["id"]);
                        data.AppId = item["app_id"].ToString();
                        data.AppName = item["app_name"].ToString();
                        data.Type = item["type"].ToString();
                        data.Revenue = Convert.ToDecimal(item["revenue"]);
                        data.Month = date.Month;
                        data.Year = date.Year;
                        dataList.Add(data);
                    }
                }

                //calculate yoy
                foreach(var data in dataList)
                {
                    decimal currentRev = data.Revenue;
                    decimal lastRev = dataList.Where(a => a.AppId == data.AppId && a.Month == data.Month && a.Year == data.Year - 1).FirstOrDefault() != null ?
                         dataList.Where(a => a.AppId == data.AppId && a.Month == data.Month && a.Year == data.Year - 1).FirstOrDefault().Revenue : 0;
                    decimal yoyGrowth = 0;
                    if (lastRev != 0)
                    {
                        yoyGrowth = (currentRev / lastRev) - 1;
                    }
                    data.YoYGrowth = yoyGrowth;
                    data.NextYoYGrowth = yoyGrowth;

                    //update table
                    var updateSql = "update tbl_sire_data set yoy_growth = @yoy_growth, next_yoy_growth = @next_yoy_growth where id = @id;";
                    Dictionary<string, object> updateParams = new Dictionary<string, object>();
                    updateParams.Add($"@yoy_growth", yoyGrowth);
                    updateParams.Add($"@next_yoy_growth", yoyGrowth);
                    updateParams.Add($"@id", data.Id);
                    DB.Upsert(updateSql, updateParams, _configuration.GetConnectionString("MysqlConnection"));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return Ok(dataList);
        }

        private async Task<string> SyncGameDataAsync()
        {

            try
            {
                var configList = GetAllIOSConfig();
                foreach (var item in configList)
                {
                    // Create an instance of HttpClient
                    using (var httpClient = new HttpClient())
                    {
                        //var uri = new Uri("https://api.sensortower.com/v1/ios/sales_report_estimates_comparison_attributes?comparison_attribute=absolute&time_range=year&measure=units&device_type=total&category=6014&date=2023-01-01&end_date=2023-12-31&limit=300&offset=700&custom_tags_mode=include_unified_apps&auth_token=ST0_yBxbkrUK_WVSXqgtK6hwk1j");
                        var uri = new Uri(item.Url);
                        // Send the POST request
                        var response = await httpClient.GetAsync(uri);

                        // Check the response status
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the response content
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var resultJson = JsonSerializer.Deserialize<List<IOSData>>(responseContent);
                            UpsertIOSData(resultJson);
                            Console.WriteLine(resultJson);
                        }
                        else
                        {
                            Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        }
                    }
                }               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            string token = string.Empty;           
            return token;
        }

        private async Task<string> SyncGameNameAsync()
        {

            try
            {
                var appList = GetAllAppId();
                foreach (var item in appList)
                {
                    // Create an instance of HttpClient
                    using (var httpClient = new HttpClient())
                    {
                        //var uri = new Uri("https://api.sensortower.com/v1/ios/apps?app_ids=1544884479,431946152&country=US&auth_token=ST0_yBxbkrUK_WVSXqgtK6hwk1j");
                        string baseUrl = "https://api.sensortower.com/v1/ios/apps?app_ids={0}&country=US&auth_token=ST0_yBxbkrUK_WVSXqgtK6hwk1j";
                        var uri = new Uri(string.Format(baseUrl, item.AppId));
                        // Send the POST request
                        var response = await httpClient.GetAsync(uri);

                        // Check the response status
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the response content
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var resultJson = JsonSerializer.Deserialize<AppNameResp>(responseContent);
                            UpsertAppName(resultJson);
                            Console.WriteLine(resultJson);
                        }
                        else
                        {
                            Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            string token = string.Empty;
            return token;
        }

        private List<AppNameData> GetAllAppId()
        {
            List<AppNameData> dataList = new List<AppNameData>();

            try
            {
                string sql = @"select distinct(a.app_id) as app_id from tbl_st_iOS a left join tbl_st_app_name b on a.app_id = b.app_id where b.app_name is null limit 10000;";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        AppNameData data = new AppNameData();
                        data.AppId = Convert.ToInt64(item["app_id"]);                      
                        dataList.Add(data);
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private List<AppNameData> GetExistingGameName()
        {
            List<AppNameData> dataList = new List<AppNameData>();

            try
            {
                string sql = @"select * from tbl_st_app_name limit 10000;";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        AppNameData data = new AppNameData();
                        data.Id = Convert.ToInt32(item["id"]);
                        data.AppId = Convert.ToInt64(item["app_id"]);
                        data.AppName = item["app_name"].ToString();
                        dataList.Add(data);
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private List<IOSConfig> GetAllIOSConfig()
        {
            List<IOSConfig> dataList = new List<IOSConfig>();

            try
            {
                string sql = @"SELECT * FROM tbl_st_iOS_config ";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        IOSConfig data = new IOSConfig();
                        data.Id = Convert.ToInt32(item["id"]);
                        data.Url = item["url"].ToString();
                        data.Limit = Convert.ToInt32(item["limit"]);
                        data.Offset = Convert.ToInt32(item["offset"]);
                        data.AuthToken = item["auth_token"].ToString();
                        data.Category = Convert.ToInt32(item["category"]);
                        data.DateFrom = Convert.ToDateTime(item["date_from"]);
                        data.DateTo = Convert.ToDateTime(item["date_to"]);

                        //merge
                        data.Url = string.Format(data.Url, data.Category, data.DateFrom.ToString("yyyy-MM-dd"), data.DateTo.ToString("yyyy-MM-dd"), data.Limit, data.Offset, data.AuthToken);
                        dataList.Add(data);
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<IOSData> GetAllIOSData()
        {
            List<IOSData> dataList = new List<IOSData>();

            try
            {
                string sql = @"SELECT * FROM tbl_st_iOS limit 10000;";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach (DataRow item in queryDT.Rows)
                    {
                        IOSData data = new IOSData();
                        data.AppId = Convert.ToInt32(item["app_id"]);                        
                        data.Date = Convert.ToDateTime(item["date"]);
                        dataList.Add(data);
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool UpsertIOSData(List<IOSData> dataList)
        {
            var existingdataList = GetAllIOSData();
            var sql = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            for (var i = 0; i < dataList.Count; i++)
            {
                if (existingdataList.Any(a => a.AppId == dataList[i].AppId && a.Date == dataList[i].Date))
                {
                    //update
                    sql += $"UPDATE tbl_st_iOS SET current_units_value = @current_units_value_{i}, comparison_units_value = @comparison_units_value_{i}, units_absolute = @units_absolute_{i}, units_delta = @units_delta_{i}, units_transformed_delta = @units_transformed_delta_{i}, current_revenue_value = @current_revenue_value_{i}, comparison_revenue_value = @comparison_revenue_value_{i}, revenue_absolute = @revenue_absolute_{i}, revenue_delta = @revenue_delta_{i}, revenue_transformed_delta = @revenue_transformed_delta_{i}, absolute = @absolute_{i}, delta = @delta_{i}, transformed_delta = @transformed_delta_{i}, country = @country_{i}, ARKit = @ARKit_{i}, ARPDAULastMonthUS = @ARPDAULastMonthUS_{i}, ARPDAULastMonthWW = @ARPDAULastMonthWW_{i}, AdvertisedOnAnyNetwork = @AdvertisedOnAnyNetwork_{i}, AdvertisesWithBannerAds = @AdvertisesWithBannerAds_{i}, AdvertisesWithFullScreenAds = @AdvertisesWithFullScreenAds_{i}, AdvertisesWithPlayableAds = @AdvertisesWithPlayableAds_{i}, AdvertisesWithVideoAds = @AdvertisesWithVideoAds_{i}, AgeLastQuarterUS = @AgeLastQuarterUS_{i}, AgeLastQuarterWW = @AgeLastQuarterWW_{i}, AllTimeDownloadsWW = @AllTimeDownloadsWW_{i}, AllTimePublisherDownloadsWW = @AllTimePublisherDownloadsWW_{i}, AllTimePublisherRevenueWW = @AllTimePublisherRevenueWW_{i}, AllTimeRevenueWW = @AllTimeRevenueWW_{i}, AppleWatchSupport = @AppleWatchSupport_{i}, BrowserDownloadsPercentageLastQUS = @BrowserDownloadsPercentageLastQUS_{i}, BrowserDownloadsPercentageLastQWW = @BrowserDownloadsPercentageLastQWW_{i}, ChangedPrice = @ChangedPrice_{i}, ContentRating = @ContentRating_{i}, CurrentUSRating = @CurrentUSRating_{i}, Day1RetentionLastQuarterUS = @Day1RetentionLastQuarterUS_{i}, Day1RetentionLastQuarterWW = @Day1RetentionLastQuarterWW_{i}, Day30RetentionLastQuarterUS = @Day30RetentionLastQuarterUS_{i}, Day30RetentionLastQuarterWW = @Day30RetentionLastQuarterWW_{i}, Day60RetentionLastQuarterUS = @Day60RetentionLastQuarterUS_{i}, Day60RetentionLastQuarterWW = @Day60RetentionLastQuarterWW_{i}, Day7RetentionLastQuarterUS = @Day7RetentionLastQuarterUS_{i}, Day7RetentionLastQuarterWW = @Day7RetentionLastQuarterWW_{i}, DownloadsFirst30DaysWW = @DownloadsFirst30DaysWW_{i}, EarliestReleaseDate = @EarliestReleaseDate_{i}, EditorsChoice = @EditorsChoice_{i}, Free = @Free_{i}, GameArtStyle = @GameArtStyle_{i}, GameCameraPOV = @GameCameraPOV_{i}, GameClass = @GameClass_{i}, GameGenre = @GameGenre_{i}, GameProductModel = @GameProductModel_{i}, GameSetting = @GameSetting_{i}, GameSubgenre = @GameSubgenre_{i}, GameTheme = @GameTheme_{i}, GendersLastQuarterUS = @GendersLastQuarterUS_{i}, GendersLastQuarterWW = @GendersLastQuarterWW_{i}, GlobalRatingCount = @GlobalRatingCount_{i}, HasVideoTrailer = @HasVideoTrailer_{i}, InAppEvents = @InAppEvents_{i}, InAppPurchases = @InAppPurchases_{i}, InactiveApp = @InactiveApp_{i}, IsUnified = @IsUnified_{i}, IsAGame = @IsAGame_{i}, Last180DaysDownloadsWW = @Last180DaysDownloadsWW_{i}, Last180DaysRevenueWW = @Last180DaysRevenueWW_{i}, Last30DaysAverageDAUUS = @Last30DaysAverageDAUUS_{i}, Last30DaysAverageDAUWW = @Last30DaysAverageDAUWW_{i}, Last30DaysDownloadsWW = @Last30DaysDownloadsWW_{i}, Last30DaysRevenueWW = @Last30DaysRevenueWW_{i}, Last4WeeksAverageWAUUS = @Last4WeeksAverageWAUUS_{i}, Last4WeeksAverageWAUWW = @Last4WeeksAverageWAUWW_{i}, LastMonthAverageMAUUS = @LastMonthAverageMAUUS_{i}, LastMonthAverageMAUWW = @LastMonthAverageMAUWW_{i}, LatestUpdateDaysAgo = @LatestUpdateDaysAgo_{i}, MessagesStickerSupport = @MessagesStickerSupport_{i}, MetaDecorationRenovation = @MetaDecorationRenovation_{i}, MetaLevels = @MetaLevels_{i}, MetaNarrativeStories = @MetaNarrativeStories_{i}, MonetizationAds = @MonetizationAds_{i}, MonetizationCurrencyBundles = @MonetizationCurrencyBundles_{i}, MonetizationFreeToPlay = @MonetizationFreeToPlay_{i}, MonetizationLiveOps = @MonetizationLiveOps_{i}, MonetizationStarterPack = @MonetizationStarterPack_{i}, MostPopularCountryByDownloads = @MostPopularCountryByDownloads_{i}, MostPopularCountryByRevenue = @MostPopularCountryByRevenue_{i}, MostPopularRegionByDownloads = @MostPopularRegionByDownloads_{i}, MostPopularRegionByRevenue = @MostPopularRegionByRevenue_{i}, OrganicDownloadsPercentageLastQUS = @OrganicDownloadsPercentageLastQUS_{i}, OrganicDownloadsPercentageLastQWW = @OrganicDownloadsPercentageLastQWW_{i}, OverallUSRating = @OverallUSRating_{i}, PaidDownloadsPercentageLastQUS = @PaidDownloadsPercentageLastQUS_{i}, PaidDownloadsPercentageLastQWW = @PaidDownloadsPercentageLastQWW_{i}, PrimaryCategory = @PrimaryCategory_{i}, PublisherCountry = @PublisherCountry_{i}, RPDAllTimeWW = @RPDAllTimeWW_{i}, RecentAppUpdate = @RecentAppUpdate_{i}, ReleaseDateJP = @ReleaseDateJP_{i}, ReleaseDateUS = @ReleaseDateUS_{i}, ReleaseDateWW = @ReleaseDateWW_{i}, ReleasedDaysAgoWW = @ReleasedDaysAgoWW_{i}, RevenueFirst30DaysWW = @RevenueFirst30DaysWW_{i}, SexualContentOrNudity = @SexualContentOrNudity_{i}, SoftLaunchDate = @SoftLaunchDate_{i}, SoftLaunchedCurrently = @SoftLaunchedCurrently_{i}, StockTicker = @StockTicker_{i}, StorefrontGameSubcategory = @StorefrontGameSubcategory_{i}, StorefrontGameSubcategorySecondary = @StorefrontGameSubcategorySecondary_{i}, USRatingCount = @USRatingCount_{i}, iOSAppFileSize = @iOSAppFileSize_{i}, iOSOfferwallsTapjoy = @iOSOfferwallsTapjoy_{i}, iOSOfferwallsIronSource = @iOSOfferwallsIronSource_{i} WHERE app_id = @app_id_{i} AND date = @date_{i};";
            parameters.Add($"@app_id_{i}", dataList[i].AppId);
            parameters.Add($"@current_units_value_{i}", dataList[i].CurrentUnitsValue);
            parameters.Add($"@comparison_units_value_{i}", dataList[i].ComparisonUnitsValue);
            parameters.Add($"@units_absolute_{i}", dataList[i].UnitsAbsolute);
            parameters.Add($"@units_delta_{i}", dataList[i].UnitsDelta);
            parameters.Add($"@units_transformed_delta_{i}", dataList[i].UnitsTransformedDelta);
            parameters.Add($"@current_revenue_value_{i}", dataList[i].CurrentRevenueValue);
            parameters.Add($"@comparison_revenue_value_{i}", dataList[i].ComparisonRevenueValue);
            parameters.Add($"@revenue_absolute_{i}", dataList[i].RevenueAbsolute);
            parameters.Add($"@revenue_delta_{i}", dataList[i].RevenueDelta);
            parameters.Add($"@revenue_transformed_delta_{i}", dataList[i].RevenueTransformedDelta);
            parameters.Add($"@absolute_{i}", dataList[i].Absolute);
            parameters.Add($"@delta_{i}", dataList[i].Delta);
            parameters.Add($"@transformed_delta_{i}", dataList[i].TransformedDelta);
            parameters.Add($"@date_{i}", dataList[i].Date);
            parameters.Add($"@country_{i}", dataList[i].Country);
            parameters.Add($"@ARKit_{i}", dataList[i].CustomTags.ARKit);
            parameters.Add($"@ARPDAULastMonthUS_{i}", dataList[i].CustomTags.ARPDAULastMonthUS);
            parameters.Add($"@ARPDAULastMonthWW_{i}", dataList[i].CustomTags.ARPDAULastMonthWW);
            parameters.Add($"@AdvertisedOnAnyNetwork_{i}", dataList[i].CustomTags.AdvertisedOnAnyNetwork);
            parameters.Add($"@AdvertisesWithBannerAds_{i}", dataList[i].CustomTags.AdvertisesWithBannerAds);
            parameters.Add($"@AdvertisesWithFullScreenAds_{i}", dataList[i].CustomTags.AdvertisesWithFullScreenAds);
            parameters.Add($"@AdvertisesWithPlayableAds_{i}", dataList[i].CustomTags.AdvertisesWithPlayableAds);
            parameters.Add($"@AdvertisesWithVideoAds_{i}", dataList[i].CustomTags.AdvertisesWithVideoAds);
            parameters.Add($"@AgeLastQuarterUS_{i}", dataList[i].CustomTags.AgeLastQuarterUS);
            parameters.Add($"@AgeLastQuarterWW_{i}", dataList[i].CustomTags.AgeLastQuarterWW);
            parameters.Add($"@AllTimeDownloadsWW_{i}", dataList[i].CustomTags.AllTimeDownloadsWW);
            parameters.Add($"@AllTimePublisherDownloadsWW_{i}", dataList[i].CustomTags.AllTimePublisherDownloadsWW);
            parameters.Add($"@AllTimePublisherRevenueWW_{i}", dataList[i].CustomTags.AllTimePublisherRevenueWW);
            parameters.Add($"@AllTimeRevenueWW_{i}", dataList[i].CustomTags.AllTimeRevenueWW);
            parameters.Add($"@AppleWatchSupport_{i}", dataList[i].CustomTags.AppleWatchSupport);
            parameters.Add($"@BrowserDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.BrowserDownloadsPercentageLastQUS);
            parameters.Add($"@BrowserDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.BrowserDownloadsPercentageLastQWW);
            parameters.Add($"@ChangedPrice_{i}", dataList[i].CustomTags.ChangedPrice);
            parameters.Add($"@ContentRating_{i}", dataList[i].CustomTags.ContentRating);
            parameters.Add($"@CurrentUSRating_{i}", dataList[i].CustomTags.CurrentUSRating);
            parameters.Add($"@Day1RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day1RetentionLastQuarterUS);
            parameters.Add($"@Day1RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day1RetentionLastQuarterWW);
            parameters.Add($"@Day30RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day30RetentionLastQuarterUS);
            parameters.Add($"@Day30RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day30RetentionLastQuarterWW);
            parameters.Add($"@Day60RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day60RetentionLastQuarterUS);
            parameters.Add($"@Day60RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day60RetentionLastQuarterWW);
            parameters.Add($"@Day7RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day7RetentionLastQuarterUS);
            parameters.Add($"@Day7RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day7RetentionLastQuarterWW);
            parameters.Add($"@DownloadsFirst30DaysWW_{i}", dataList[i].CustomTags.DownloadsFirst30DaysWW);
            parameters.Add($"@EarliestReleaseDate_{i}", dataList[i].CustomTags.EarliestReleaseDate);
            parameters.Add($"@EditorsChoice_{i}", dataList[i].CustomTags.EditorsChoice);
            parameters.Add($"@Free_{i}", dataList[i].CustomTags.Free);
            parameters.Add($"@GameArtStyle_{i}", dataList[i].CustomTags.GameArtStyle);
            parameters.Add($"@GameCameraPOV_{i}", dataList[i].CustomTags.GameCameraPOV);
            parameters.Add($"@GameClass_{i}", dataList[i].CustomTags.GameClass);
            parameters.Add($"@GameGenre_{i}", dataList[i].CustomTags.GameGenre);
            parameters.Add($"@GameProductModel_{i}", dataList[i].CustomTags.GameProductModel);
            parameters.Add($"@GameSetting_{i}", dataList[i].CustomTags.GameSetting);
            parameters.Add($"@GameSubgenre_{i}", dataList[i].CustomTags.GameSubgenre);
            parameters.Add($"@GameTheme_{i}", dataList[i].CustomTags.GameTheme);
            parameters.Add($"@GendersLastQuarterUS_{i}", dataList[i].CustomTags.GendersLastQuarterUS);
            parameters.Add($"@GendersLastQuarterWW_{i}", dataList[i].CustomTags.GendersLastQuarterWW);
            parameters.Add($"@GlobalRatingCount_{i}", dataList[i].CustomTags.GlobalRatingCount);
            parameters.Add($"@HasVideoTrailer_{i}", dataList[i].CustomTags.HasVideoTrailer);
            parameters.Add($"@InAppEvents_{i}", dataList[i].CustomTags.InAppEvents);
            parameters.Add($"@InAppPurchases_{i}", dataList[i].CustomTags.InAppPurchases);
            parameters.Add($"@InactiveApp_{i}", dataList[i].CustomTags.InactiveApp);
            parameters.Add($"@IsUnified_{i}", dataList[i].CustomTags.IsUnified);
            parameters.Add($"@IsAGame_{i}", dataList[i].CustomTags.IsAGame);
            parameters.Add($"@Last180DaysDownloadsWW_{i}", dataList[i].CustomTags.Last180DaysDownloadsWW);
            parameters.Add($"@Last180DaysRevenueWW_{i}", dataList[i].CustomTags.Last180DaysRevenueWW);
            parameters.Add($"@Last30DaysAverageDAUUS_{i}", dataList[i].CustomTags.Last30DaysAverageDAUUS);
            parameters.Add($"@Last30DaysAverageDAUWW_{i}", dataList[i].CustomTags.Last30DaysAverageDAUWW);
            parameters.Add($"@Last30DaysDownloadsWW_{i}", dataList[i].CustomTags.Last30DaysDownloadsWW);
            parameters.Add($"@Last30DaysRevenueWW_{i}", dataList[i].CustomTags.Last30DaysRevenueWW);
            parameters.Add($"@Last4WeeksAverageWAUUS_{i}", dataList[i].CustomTags.Last4WeeksAverageWAUUS);
            parameters.Add($"@Last4WeeksAverageWAUWW_{i}", dataList[i].CustomTags.Last4WeeksAverageWAUWW);
            parameters.Add($"@LastMonthAverageMAUUS_{i}", dataList[i].CustomTags.LastMonthAverageMAUUS);
            parameters.Add($"@LastMonthAverageMAUWW_{i}", dataList[i].CustomTags.LastMonthAverageMAUWW);
            parameters.Add($"@LatestUpdateDaysAgo_{i}", dataList[i].CustomTags.LatestUpdateDaysAgo);
            parameters.Add($"@MessagesStickerSupport_{i}", dataList[i].CustomTags.MessagesStickerSupport);
            parameters.Add($"@MetaDecorationRenovation_{i}", dataList[i].CustomTags.MetaDecorationRenovation);
            parameters.Add($"@MetaLevels_{i}", dataList[i].CustomTags.MetaLevels);
            parameters.Add($"@MetaNarrativeStories_{i}", dataList[i].CustomTags.MetaNarrativeStories);
            parameters.Add($"@MonetizationAds_{i}", dataList[i].CustomTags.MonetizationAds);
            parameters.Add($"@MonetizationCurrencyBundles_{i}", dataList[i].CustomTags.MonetizationCurrencyBundles);
            parameters.Add($"@MonetizationFreeToPlay_{i}", dataList[i].CustomTags.MonetizationFreeToPlay);
            parameters.Add($"@MonetizationLiveOps_{i}", dataList[i].CustomTags.MonetizationLiveOps);
            parameters.Add($"@MonetizationStarterPack_{i}", dataList[i].CustomTags.MonetizationStarterPack);
            parameters.Add($"@MostPopularCountryByDownloads_{i}", dataList[i].CustomTags.MostPopularCountryByDownloads);
            parameters.Add($"@MostPopularCountryByRevenue_{i}", dataList[i].CustomTags.MostPopularCountryByRevenue);
            parameters.Add($"@MostPopularRegionByDownloads_{i}", dataList[i].CustomTags.MostPopularRegionByDownloads);
            parameters.Add($"@MostPopularRegionByRevenue_{i}", dataList[i].CustomTags.MostPopularRegionByRevenue);
            parameters.Add($"@OrganicDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.OrganicDownloadsPercentageLastQUS);
            parameters.Add($"@OrganicDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.OrganicDownloadsPercentageLastQWW);
            parameters.Add($"@OverallUSRating_{i}", dataList[i].CustomTags.OverallUSRating);
            parameters.Add($"@PaidDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.PaidDownloadsPercentageLastQUS);
            parameters.Add($"@PaidDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.PaidDownloadsPercentageLastQWW);
            parameters.Add($"@PrimaryCategory_{i}", dataList[i].CustomTags.PrimaryCategory);
            parameters.Add($"@PublisherCountry_{i}", dataList[i].CustomTags.PublisherCountry);
            parameters.Add($"@RPDAllTimeWW_{i}", dataList[i].CustomTags.RPDAllTimeWW);
            parameters.Add($"@RecentAppUpdate_{i}", dataList[i].CustomTags.RecentAppUpdate);
            parameters.Add($"@ReleaseDateJP_{i}", dataList[i].CustomTags.ReleaseDateJP);
            parameters.Add($"@ReleaseDateUS_{i}", dataList[i].CustomTags.ReleaseDateUS);
            parameters.Add($"@ReleaseDateWW_{i}", dataList[i].CustomTags.ReleaseDateWW);
            parameters.Add($"@ReleasedDaysAgoWW_{i}", dataList[i].CustomTags.ReleasedDaysAgoWW);
            parameters.Add($"@RevenueFirst30DaysWW_{i}", dataList[i].CustomTags.RevenueFirst30DaysWW);
            parameters.Add($"@SexualContentOrNudity_{i}", dataList[i].CustomTags.SexualContentOrNudity);
            parameters.Add($"@SoftLaunchDate_{i}", dataList[i].CustomTags.SoftLaunchDate);
            parameters.Add($"@SoftLaunchedCurrently_{i}", dataList[i].CustomTags.SoftLaunchedCurrently);
            parameters.Add($"@StockTicker_{i}", dataList[i].CustomTags.StockTicker);
            parameters.Add($"@StorefrontGameSubcategory_{i}", dataList[i].CustomTags.StorefrontGameSubcategory);
            parameters.Add($"@StorefrontGameSubcategorySecondary_{i}", dataList[i].CustomTags.StorefrontGameSubcategorySecondary);
            parameters.Add($"@USRatingCount_{i}", dataList[i].CustomTags.USRatingCount);
            parameters.Add($"@iOSAppFileSize_{i}", dataList[i].CustomTags.iOSAppFileSize);
            parameters.Add($"@iOSOfferwallsTapjoy_{i}", dataList[i].CustomTags.iOSOfferwallsTapjoy);
            parameters.Add($"@iOSOfferwallsIronSource_{i}", dataList[i].CustomTags.iOSOfferwallsIronSource);
        }
                else
                {
                    //insert
                    sql += $"INSERT INTO tbl_st_iOS (app_id, current_units_value, comparison_units_value, units_absolute, units_delta, units_transformed_delta, current_revenue_value, comparison_revenue_value, revenue_absolute, revenue_delta, revenue_transformed_delta, absolute, delta, transformed_delta, date, country, ARKit, ARPDAULastMonthUS, ARPDAULastMonthWW, AdvertisedOnAnyNetwork, AdvertisesWithBannerAds, AdvertisesWithFullScreenAds, AdvertisesWithPlayableAds, AdvertisesWithVideoAds, AgeLastQuarterUS, AgeLastQuarterWW, AllTimeDownloadsWW, AllTimePublisherDownloadsWW, AllTimePublisherRevenueWW, AllTimeRevenueWW, AppleWatchSupport, BrowserDownloadsPercentageLastQUS, BrowserDownloadsPercentageLastQWW, ChangedPrice, ContentRating, CurrentUSRating, Day1RetentionLastQuarterUS, Day1RetentionLastQuarterWW, Day30RetentionLastQuarterUS, Day30RetentionLastQuarterWW, Day60RetentionLastQuarterUS, Day60RetentionLastQuarterWW, Day7RetentionLastQuarterUS, Day7RetentionLastQuarterWW, DownloadsFirst30DaysWW, EarliestReleaseDate, EditorsChoice, Free, GameArtStyle, GameCameraPOV, GameClass, GameGenre, GameProductModel, GameSetting, GameSubgenre, GameTheme, GendersLastQuarterUS, GendersLastQuarterWW, GlobalRatingCount, HasVideoTrailer, InAppEvents, InAppPurchases, InactiveApp, IsUnified, IsAGame, Last180DaysDownloadsWW, Last180DaysRevenueWW, Last30DaysAverageDAUUS, Last30DaysAverageDAUWW, Last30DaysDownloadsWW, Last30DaysRevenueWW, Last4WeeksAverageWAUUS, Last4WeeksAverageWAUWW, LastMonthAverageMAUUS, LastMonthAverageMAUWW, LatestUpdateDaysAgo, MessagesStickerSupport, MetaDecorationRenovation, MetaLevels, MetaNarrativeStories, MonetizationAds, MonetizationCurrencyBundles, MonetizationFreeToPlay, MonetizationLiveOps, MonetizationStarterPack, MostPopularCountryByDownloads, MostPopularCountryByRevenue, MostPopularRegionByDownloads, MostPopularRegionByRevenue, OrganicDownloadsPercentageLastQUS, OrganicDownloadsPercentageLastQWW, OverallUSRating, PaidDownloadsPercentageLastQUS, PaidDownloadsPercentageLastQWW, PrimaryCategory, PublisherCountry, RPDAllTimeWW, RecentAppUpdate, ReleaseDateJP, ReleaseDateUS, ReleaseDateWW, ReleasedDaysAgoWW, RevenueFirst30DaysWW, SexualContentOrNudity, SoftLaunchDate, SoftLaunchedCurrently, StockTicker, StorefrontGameSubcategory, StorefrontGameSubcategorySecondary, USRatingCount, iOSAppFileSize, iOSOfferwallsTapjoy, iOSOfferwallsIronSource ) VALUES (@app_id_{i}, @current_units_value_{i}, @comparison_units_value_{i}, @units_absolute_{i}, @units_delta_{i}, @units_transformed_delta_{i}, @current_revenue_value_{i}, @comparison_revenue_value_{i}, @revenue_absolute_{i}, @revenue_delta_{i}, @revenue_transformed_delta_{i}, @absolute_{i}, @delta_{i}, @transformed_delta_{i}, @date_{i}, @country_{i}, @ARKit_{i}, @ARPDAULastMonthUS_{i}, @ARPDAULastMonthWW_{i}, @AdvertisedOnAnyNetwork_{i}, @AdvertisesWithBannerAds_{i}, @AdvertisesWithFullScreenAds_{i}, @AdvertisesWithPlayableAds_{i}, @AdvertisesWithVideoAds_{i}, @AgeLastQuarterUS_{i}, @AgeLastQuarterWW_{i}, @AllTimeDownloadsWW_{i}, @AllTimePublisherDownloadsWW_{i}, @AllTimePublisherRevenueWW_{i}, @AllTimeRevenueWW_{i}, @AppleWatchSupport_{i}, @BrowserDownloadsPercentageLastQUS_{i}, @BrowserDownloadsPercentageLastQWW_{i}, @ChangedPrice_{i}, @ContentRating_{i}, @CurrentUSRating_{i}, @Day1RetentionLastQuarterUS_{i}, @Day1RetentionLastQuarterWW_{i}, @Day30RetentionLastQuarterUS_{i}, @Day30RetentionLastQuarterWW_{i}, @Day60RetentionLastQuarterUS_{i}, @Day60RetentionLastQuarterWW_{i}, @Day7RetentionLastQuarterUS_{i}, @Day7RetentionLastQuarterWW_{i}, @DownloadsFirst30DaysWW_{i}, @EarliestReleaseDate_{i}, @EditorsChoice_{i}, @Free_{i}, @GameArtStyle_{i}, @GameCameraPOV_{i}, @GameClass_{i}, @GameGenre_{i}, @GameProductModel_{i}, @GameSetting_{i}, @GameSubgenre_{i}, @GameTheme_{i}, @GendersLastQuarterUS_{i}, @GendersLastQuarterWW_{i}, @GlobalRatingCount_{i}, @HasVideoTrailer_{i}, @InAppEvents_{i}, @InAppPurchases_{i}, @InactiveApp_{i}, @IsUnified_{i}, @IsAGame_{i}, @Last180DaysDownloadsWW_{i}, @Last180DaysRevenueWW_{i}, @Last30DaysAverageDAUUS_{i}, @Last30DaysAverageDAUWW_{i}, @Last30DaysDownloadsWW_{i}, @Last30DaysRevenueWW_{i}, @Last4WeeksAverageWAUUS_{i}, @Last4WeeksAverageWAUWW_{i}, @LastMonthAverageMAUUS_{i}, @LastMonthAverageMAUWW_{i}, @LatestUpdateDaysAgo_{i}, @MessagesStickerSupport_{i}, @MetaDecorationRenovation_{i}, @MetaLevels_{i}, @MetaNarrativeStories_{i}, @MonetizationAds_{i}, @MonetizationCurrencyBundles_{i}, @MonetizationFreeToPlay_{i}, @MonetizationLiveOps_{i}, @MonetizationStarterPack_{i}, @MostPopularCountryByDownloads_{i}, @MostPopularCountryByRevenue_{i}, @MostPopularRegionByDownloads_{i}, @MostPopularRegionByRevenue_{i}, @OrganicDownloadsPercentageLastQUS_{i}, @OrganicDownloadsPercentageLastQWW_{i}, @OverallUSRating_{i}, @PaidDownloadsPercentageLastQUS_{i}, @PaidDownloadsPercentageLastQWW_{i}, @PrimaryCategory_{i}, @PublisherCountry_{i}, @RPDAllTimeWW_{i}, @RecentAppUpdate_{i}, @ReleaseDateJP_{i}, @ReleaseDateUS_{i}, @ReleaseDateWW_{i}, @ReleasedDaysAgoWW_{i}, @RevenueFirst30DaysWW_{i}, @SexualContentOrNudity_{i}, @SoftLaunchDate_{i}, @SoftLaunchedCurrently_{i}, @StockTicker_{i}, @StorefrontGameSubcategory_{i}, @StorefrontGameSubcategorySecondary_{i}, @USRatingCount_{i}, @iOSAppFileSize_{i}, @iOSOfferwallsTapjoy_{i}, @iOSOfferwallsIronSource_{i});";
                    parameters.Add($"@app_id_{i}", dataList[i].AppId);
                    parameters.Add($"@current_units_value_{i}", dataList[i].CurrentUnitsValue);
                    parameters.Add($"@comparison_units_value_{i}", dataList[i].ComparisonUnitsValue);
                    parameters.Add($"@units_absolute_{i}", dataList[i].UnitsAbsolute);
                    parameters.Add($"@units_delta_{i}", dataList[i].UnitsDelta);
                    parameters.Add($"@units_transformed_delta_{i}", dataList[i].UnitsTransformedDelta);
                    parameters.Add($"@current_revenue_value_{i}", dataList[i].CurrentRevenueValue);
                    parameters.Add($"@comparison_revenue_value_{i}", dataList[i].ComparisonRevenueValue);
                    parameters.Add($"@revenue_absolute_{i}", dataList[i].RevenueAbsolute);
                    parameters.Add($"@revenue_delta_{i}", dataList[i].RevenueDelta);
                    parameters.Add($"@revenue_transformed_delta_{i}", dataList[i].RevenueTransformedDelta);
                    parameters.Add($"@absolute_{i}", dataList[i].Absolute);
                    parameters.Add($"@delta_{i}", dataList[i].Delta);
                    parameters.Add($"@transformed_delta_{i}", dataList[i].TransformedDelta);
                    parameters.Add($"@date_{i}", dataList[i].Date);
                    parameters.Add($"@country_{i}", dataList[i].Country);
                    parameters.Add($"@ARKit_{i}", dataList[i].CustomTags.ARKit);
                    parameters.Add($"@ARPDAULastMonthUS_{i}", dataList[i].CustomTags.ARPDAULastMonthUS);
                    parameters.Add($"@ARPDAULastMonthWW_{i}", dataList[i].CustomTags.ARPDAULastMonthWW);
                    parameters.Add($"@AdvertisedOnAnyNetwork_{i}", dataList[i].CustomTags.AdvertisedOnAnyNetwork);
                    parameters.Add($"@AdvertisesWithBannerAds_{i}", dataList[i].CustomTags.AdvertisesWithBannerAds);
                    parameters.Add($"@AdvertisesWithFullScreenAds_{i}", dataList[i].CustomTags.AdvertisesWithFullScreenAds);
                    parameters.Add($"@AdvertisesWithPlayableAds_{i}", dataList[i].CustomTags.AdvertisesWithPlayableAds);
                    parameters.Add($"@AdvertisesWithVideoAds_{i}", dataList[i].CustomTags.AdvertisesWithVideoAds);
                    parameters.Add($"@AgeLastQuarterUS_{i}", dataList[i].CustomTags.AgeLastQuarterUS);
                    parameters.Add($"@AgeLastQuarterWW_{i}", dataList[i].CustomTags.AgeLastQuarterWW);
                    parameters.Add($"@AllTimeDownloadsWW_{i}", dataList[i].CustomTags.AllTimeDownloadsWW);
                    parameters.Add($"@AllTimePublisherDownloadsWW_{i}", dataList[i].CustomTags.AllTimePublisherDownloadsWW);
                    parameters.Add($"@AllTimePublisherRevenueWW_{i}", dataList[i].CustomTags.AllTimePublisherRevenueWW);
                    parameters.Add($"@AllTimeRevenueWW_{i}", dataList[i].CustomTags.AllTimeRevenueWW);
                    parameters.Add($"@AppleWatchSupport_{i}", dataList[i].CustomTags.AppleWatchSupport);
                    parameters.Add($"@BrowserDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.BrowserDownloadsPercentageLastQUS);
                    parameters.Add($"@BrowserDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.BrowserDownloadsPercentageLastQWW);
                    parameters.Add($"@ChangedPrice_{i}", dataList[i].CustomTags.ChangedPrice);
                    parameters.Add($"@ContentRating_{i}", dataList[i].CustomTags.ContentRating);
                    parameters.Add($"@CurrentUSRating_{i}", dataList[i].CustomTags.CurrentUSRating);
                    parameters.Add($"@Day1RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day1RetentionLastQuarterUS);
                    parameters.Add($"@Day1RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day1RetentionLastQuarterWW);
                    parameters.Add($"@Day30RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day30RetentionLastQuarterUS);
                    parameters.Add($"@Day30RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day30RetentionLastQuarterWW);
                    parameters.Add($"@Day60RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day60RetentionLastQuarterUS);
                    parameters.Add($"@Day60RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day60RetentionLastQuarterWW);
                    parameters.Add($"@Day7RetentionLastQuarterUS_{i}", dataList[i].CustomTags.Day7RetentionLastQuarterUS);
                    parameters.Add($"@Day7RetentionLastQuarterWW_{i}", dataList[i].CustomTags.Day7RetentionLastQuarterWW);
                    parameters.Add($"@DownloadsFirst30DaysWW_{i}", dataList[i].CustomTags.DownloadsFirst30DaysWW);
                    parameters.Add($"@EarliestReleaseDate_{i}", dataList[i].CustomTags.EarliestReleaseDate);
                    parameters.Add($"@EditorsChoice_{i}", dataList[i].CustomTags.EditorsChoice);
                    parameters.Add($"@Free_{i}", dataList[i].CustomTags.Free);
                    parameters.Add($"@GameArtStyle_{i}", dataList[i].CustomTags.GameArtStyle);
                    parameters.Add($"@GameCameraPOV_{i}", dataList[i].CustomTags.GameCameraPOV);
                    parameters.Add($"@GameClass_{i}", dataList[i].CustomTags.GameClass);
                    parameters.Add($"@GameGenre_{i}", dataList[i].CustomTags.GameGenre);
                    parameters.Add($"@GameProductModel_{i}", dataList[i].CustomTags.GameProductModel);
                    parameters.Add($"@GameSetting_{i}", dataList[i].CustomTags.GameSetting);
                    parameters.Add($"@GameSubgenre_{i}", dataList[i].CustomTags.GameSubgenre);
                    parameters.Add($"@GameTheme_{i}", dataList[i].CustomTags.GameTheme);
                    parameters.Add($"@GendersLastQuarterUS_{i}", dataList[i].CustomTags.GendersLastQuarterUS);
                    parameters.Add($"@GendersLastQuarterWW_{i}", dataList[i].CustomTags.GendersLastQuarterWW);
                    parameters.Add($"@GlobalRatingCount_{i}", dataList[i].CustomTags.GlobalRatingCount);
                    parameters.Add($"@HasVideoTrailer_{i}", dataList[i].CustomTags.HasVideoTrailer);
                    parameters.Add($"@InAppEvents_{i}", dataList[i].CustomTags.InAppEvents);
                    parameters.Add($"@InAppPurchases_{i}", dataList[i].CustomTags.InAppPurchases);
                    parameters.Add($"@InactiveApp_{i}", dataList[i].CustomTags.InactiveApp);
                    parameters.Add($"@IsUnified_{i}", dataList[i].CustomTags.IsUnified);
                    parameters.Add($"@IsAGame_{i}", dataList[i].CustomTags.IsAGame);
                    parameters.Add($"@Last180DaysDownloadsWW_{i}", dataList[i].CustomTags.Last180DaysDownloadsWW);
                    parameters.Add($"@Last180DaysRevenueWW_{i}", dataList[i].CustomTags.Last180DaysRevenueWW);
                    parameters.Add($"@Last30DaysAverageDAUUS_{i}", dataList[i].CustomTags.Last30DaysAverageDAUUS);
                    parameters.Add($"@Last30DaysAverageDAUWW_{i}", dataList[i].CustomTags.Last30DaysAverageDAUWW);
                    parameters.Add($"@Last30DaysDownloadsWW_{i}", dataList[i].CustomTags.Last30DaysDownloadsWW);
                    parameters.Add($"@Last30DaysRevenueWW_{i}", dataList[i].CustomTags.Last30DaysRevenueWW);
                    parameters.Add($"@Last4WeeksAverageWAUUS_{i}", dataList[i].CustomTags.Last4WeeksAverageWAUUS);
                    parameters.Add($"@Last4WeeksAverageWAUWW_{i}", dataList[i].CustomTags.Last4WeeksAverageWAUWW);
                    parameters.Add($"@LastMonthAverageMAUUS_{i}", dataList[i].CustomTags.LastMonthAverageMAUUS);
                    parameters.Add($"@LastMonthAverageMAUWW_{i}", dataList[i].CustomTags.LastMonthAverageMAUWW);
                    parameters.Add($"@LatestUpdateDaysAgo_{i}", dataList[i].CustomTags.LatestUpdateDaysAgo);
                    parameters.Add($"@MessagesStickerSupport_{i}", dataList[i].CustomTags.MessagesStickerSupport);
                    parameters.Add($"@MetaDecorationRenovation_{i}", dataList[i].CustomTags.MetaDecorationRenovation);
                    parameters.Add($"@MetaLevels_{i}", dataList[i].CustomTags.MetaLevels);
                    parameters.Add($"@MetaNarrativeStories_{i}", dataList[i].CustomTags.MetaNarrativeStories);
                    parameters.Add($"@MonetizationAds_{i}", dataList[i].CustomTags.MonetizationAds);
                    parameters.Add($"@MonetizationCurrencyBundles_{i}", dataList[i].CustomTags.MonetizationCurrencyBundles);
                    parameters.Add($"@MonetizationFreeToPlay_{i}", dataList[i].CustomTags.MonetizationFreeToPlay);
                    parameters.Add($"@MonetizationLiveOps_{i}", dataList[i].CustomTags.MonetizationLiveOps);
                    parameters.Add($"@MonetizationStarterPack_{i}", dataList[i].CustomTags.MonetizationStarterPack);
                    parameters.Add($"@MostPopularCountryByDownloads_{i}", dataList[i].CustomTags.MostPopularCountryByDownloads);
                    parameters.Add($"@MostPopularCountryByRevenue_{i}", dataList[i].CustomTags.MostPopularCountryByRevenue);
                    parameters.Add($"@MostPopularRegionByDownloads_{i}", dataList[i].CustomTags.MostPopularRegionByDownloads);
                    parameters.Add($"@MostPopularRegionByRevenue_{i}", dataList[i].CustomTags.MostPopularRegionByRevenue);
                    parameters.Add($"@OrganicDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.OrganicDownloadsPercentageLastQUS);
                    parameters.Add($"@OrganicDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.OrganicDownloadsPercentageLastQWW);
                    parameters.Add($"@OverallUSRating_{i}", dataList[i].CustomTags.OverallUSRating);
                    parameters.Add($"@PaidDownloadsPercentageLastQUS_{i}", dataList[i].CustomTags.PaidDownloadsPercentageLastQUS);
                    parameters.Add($"@PaidDownloadsPercentageLastQWW_{i}", dataList[i].CustomTags.PaidDownloadsPercentageLastQWW);
                    parameters.Add($"@PrimaryCategory_{i}", dataList[i].CustomTags.PrimaryCategory);
                    parameters.Add($"@PublisherCountry_{i}", dataList[i].CustomTags.PublisherCountry);
                    parameters.Add($"@RPDAllTimeWW_{i}", dataList[i].CustomTags.RPDAllTimeWW);
                    parameters.Add($"@RecentAppUpdate_{i}", dataList[i].CustomTags.RecentAppUpdate);
                    parameters.Add($"@ReleaseDateJP_{i}", dataList[i].CustomTags.ReleaseDateJP);
                    parameters.Add($"@ReleaseDateUS_{i}", dataList[i].CustomTags.ReleaseDateUS);
                    parameters.Add($"@ReleaseDateWW_{i}", dataList[i].CustomTags.ReleaseDateWW);
                    parameters.Add($"@ReleasedDaysAgoWW_{i}", dataList[i].CustomTags.ReleasedDaysAgoWW);
                    parameters.Add($"@RevenueFirst30DaysWW_{i}", dataList[i].CustomTags.RevenueFirst30DaysWW);
                    parameters.Add($"@SexualContentOrNudity_{i}", dataList[i].CustomTags.SexualContentOrNudity);
                    parameters.Add($"@SoftLaunchDate_{i}", dataList[i].CustomTags.SoftLaunchDate);
                    parameters.Add($"@SoftLaunchedCurrently_{i}", dataList[i].CustomTags.SoftLaunchedCurrently);
                    parameters.Add($"@StockTicker_{i}", dataList[i].CustomTags.StockTicker);
                    parameters.Add($"@StorefrontGameSubcategory_{i}", dataList[i].CustomTags.StorefrontGameSubcategory);
                    parameters.Add($"@StorefrontGameSubcategorySecondary_{i}", dataList[i].CustomTags.StorefrontGameSubcategorySecondary);
                    parameters.Add($"@USRatingCount_{i}", dataList[i].CustomTags.USRatingCount);
                    parameters.Add($"@iOSAppFileSize_{i}", dataList[i].CustomTags.iOSAppFileSize);
                    parameters.Add($"@iOSOfferwallsTapjoy_{i}", dataList[i].CustomTags.iOSOfferwallsTapjoy);
                    parameters.Add($"@iOSOfferwallsIronSource_{i}", dataList[i].CustomTags.iOSOfferwallsIronSource);
                }
            }
            

            DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
            return true;
        }
        private bool UpsertAppName(AppNameResp resp)
        {
            var existingdataList = GetExistingGameName();

            var dataList = resp.apps;
            var sql = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (dataList.Count != 0)
            {
                for (var i = 0; i < dataList.Count; i++)
                {
                    if (existingdataList.Any(a => a.AppId == dataList[i].AppId))
                    {
                        //update
                        sql += $"UPDATE tbl_st_app_name SET app_name = @app_name_{i} WHERE app_id = @app_id_{i} ;";
                        parameters.Add($"@app_name_{i}", dataList[i].AppName);
                        parameters.Add($"@app_id_{i}", dataList[i].Id);
                    }
                    else
                    {
                        //insert
                        sql += $"INSERT INTO tbl_st_app_name (app_id, app_name) VALUES (@app_id_{i}, @app_name_{i});";
                        parameters.Add($"@app_name_{i}", dataList[i].AppName);
                        parameters.Add($"@app_id_{i}", dataList[i].AppId);
                    }
                }

                DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
            }           
            return true;
        }
        private bool InsertSireData(List<SensorTowerCsvData> dataList)
        {
            var sql = string.Empty;
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            for (var i = 0; i < dataList.Count; i++)
            {
                //insert
                sql += $"INSERT INTO tbl_sire_data (app_id, app_name, revenue, type, data_date) VALUES (@app_id_{i}, @app_name_{i}, @revenue_{i}, @type_{i}, @data_date_{i});";
                parameters.Add($"@app_id_{i}", dataList[i].AppId);
                parameters.Add($"@app_name_{i}", dataList[i].AppName);
                parameters.Add($"@revenue_{i}", dataList[i].Revenue);
                parameters.Add($"@type_{i}", dataList[i].Type);
                parameters.Add($"@data_date_{i}", dataList[i].DataDate);
            }

            DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
            return true;
        }

    }
}
