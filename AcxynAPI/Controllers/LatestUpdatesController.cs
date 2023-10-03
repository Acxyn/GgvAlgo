
using AcxynAPI.Common;
using AcxynAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AcxynAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LatestUpdatesController : Controller
    {
        private readonly ILogger<LatestUpdatesController> _logger;
        private readonly IConfiguration _configuration;

        public LatestUpdatesController(ILogger<LatestUpdatesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet()]
        public List<LatestUpdatesModel> GetLatestUpdates()
        {
            List<LatestUpdatesModel> latestUpdatesList = new List<LatestUpdatesModel>();

            //Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("@api_key", pLayerIAP.ApiKey);

            string sql = @"SELECT * FROM tbl_latest_updates ";

            var queryDT = DB.Select(sql, null, _configuration.GetConnectionString("MysqlConnection"));

            if (queryDT.Rows.Count > 0)
            {
                foreach (DataRow item in queryDT.Rows)
                {
                    LatestUpdatesModel latestUpdates = new LatestUpdatesModel();
                    latestUpdates.title = item["title"].ToString();
                    latestUpdates.description = item["description"].ToString();
                    latestUpdates.bannerUrl = item["banner_url"].ToString();
                    latestUpdates.redirectUrl = item["redirect_url"].ToString();
                    latestUpdates.publishDate = Convert.ToDateTime(item["publish_date"]);
                    latestUpdatesList.Add(latestUpdates);

                }
            }

            return latestUpdatesList;
        }
    }
}
