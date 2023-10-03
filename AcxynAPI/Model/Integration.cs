using System;
using System.Collections.Generic;

namespace AcxynAPI.Model
{
    public class LoginReq
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class LoginResp
    {
        public string token { get; set; }
        public LoginRespData data { get; set; }
        public bool error { get; set; }
    }
    public class LoginRespData
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string wallet_address { get; set; }
        public int id { get; set; }
    }

    public class UpdateGGVReq
    {
        public int pe_valuation { get; set; }
        public int player_metric { get; set; }
        public int dev_activity { get; set; }
        public int gmv { get; set; }
        public int ggv { get; set; }
    }
    public class UpdateGGVResp
    {
        public bool error { get; set; }
        public string msg { get; set; }
    }

    public class GameResp
    {
        public bool error { get; set; }
        public GameRespData data { get; set; }
    }

    public class GameRespData
    {
        public int id { get; set; }
        public int ggv { get; set; }
    }
    public class GameListResp
    {
        public bool error { get; set; }
        public List<GameListRespData> data { get; set; }
    }
    public class GameListRespData
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal bucket_size { get; set; }
        public decimal transaction_volume { get; set; }
        public int integrated_function { get; set; }
        public int avg_player_retention { get; set; }
        public int avg_player_count { get; set; }
        public int avg_play_time { get; set; }
        public int avg_ad_rev { get; set; }
        public int avg_rpu { get; set; }
        public decimal network_value { get; set; }
    }
}