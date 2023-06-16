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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AcxynAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OraclesController : ControllerBase
    {
        private readonly ILogger<OraclesController> _logger;
        private readonly IConfiguration _configuration;

        public OraclesController(ILogger<OraclesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("Example")]
        public List<Example> Example()
        {
            List<Example> response = new List<Example>();

            try
            {           
                string sql = @"SELECT * FROM tbl_example";
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    foreach(var data in queryDT.Rows)
                    {
                        foreach (DataRow item in queryDT.Rows)
                        {
                            Example example = new Example();
                            example.NFTAddress = item["nft_address"].ToString();
                            example.Value = Convert.ToInt32(item["value"]);
                            response.Add(example);

                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return response;
            }     
        }

        [HttpPost("Example")]
        public Response Example(Example example)
        {
            Response response = new Response();
            response.Success = true;
            try
            {
                string sql = "insert into tbl_example " +
                   "(nft_address, value) values " +
                   "(@nft_address, @value) ";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@nft_address", example.NFTAddress);
                parameters.Add("@value", example.Value);

                DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorCode = "-1";
                response.Message = ex.ToString().Substring(0, 1000);
                return response;
            }
        }
    }
}
