using AcxynAPI.Common;
using AcxynAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    public class PlayerController : ControllerBase
    {
        private readonly ILogger<PlayerController> _logger;
        private readonly IConfiguration _configuration;

        public PlayerController(ILogger<PlayerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("PlayerLoginLogout")]
        public Response PlayerLoginLogout(PlayerLoginLogout playerLoginLogout)
        {
            Response response = new Response();

            //validation 
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@api_key", playerLoginLogout.ApiKey);

            string sql = @"SELECT * FROM tbl_api a where is_active = 1 and api_key = @api_key ";

            var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

            if (queryDT.Rows.Count == 0)
            {
                response.Success = false;
                response.ErrorCode = "101";
                response.Message = "Invalid API key!";
                return response;
            }

            try
            {
                sql = @"SELECT * FROM tbl_player_login a where username = @username and api_key = @api_key and logout_date is null ";

                parameters.Clear();
                parameters.Add("@username", playerLoginLogout.Username);
                parameters.Add("@api_key", playerLoginLogout.ApiKey);

                queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

                if (queryDT.Rows.Count > 0)
                {
                    sql = "update tbl_player_login set logout_date = @logout_date, duration_sec = TIMESTAMPDIFF(SECOND, login_date, @logout_date) where id = @id";
                    var item = queryDT.Rows[0];
                    parameters.Clear();
                    parameters.Add("@id", item["id"]);
                    parameters.Add("@logout_date", DateTime.Now);

                    DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
                }
                else
                {
                    sql = "insert into tbl_player_login " +
                     "(username, api_key, login_date) values " +
                     "(@username, @api_key, @login_date) ";

                    parameters.Clear();
                    parameters.Add("@username", playerLoginLogout.Username);
                    parameters.Add("@api_key", playerLoginLogout.ApiKey);
                    parameters.Add("@login_date", DateTime.Now);

                    DB.Upsert(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));
                }


              
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

        [HttpPost("PlayerIAP")]
        public Response PlayerIAP(PLayerIAP pLayerIAP)
        {
            Response response = new Response();

            //validation 
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@api_key", pLayerIAP.ApiKey);

            string sql = @"SELECT * FROM tbl_api a where is_active = 1 and api_key = @api_key ";

            var queryDT = DB.Select(sql, parameters, _configuration.GetConnectionString("MysqlConnection"));

            if (queryDT.Rows.Count == 0)
            {
                response.Success = false;
                response.ErrorCode = "101";
                response.Message = "Invalid API key!";
                return response;
            }

            try
            {
                sql = "insert into tbl_player_iap " +
                   "(username, api_key, created_date, item_code, item_desc, amount) values " +
                   "(@username, @api_key, @created_date, @item_code, @item_desc, @amount) ";

                parameters.Clear();
                parameters.Add("@username", pLayerIAP.Username);
                parameters.Add("@api_key", pLayerIAP.ApiKey);
                parameters.Add("@created_date", DateTime.Now);
                parameters.Add("@item_code", pLayerIAP.ItemCode);
                parameters.Add("@item_desc", pLayerIAP.ItemDescription);
                parameters.Add("@amount", pLayerIAP.Amount);

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
