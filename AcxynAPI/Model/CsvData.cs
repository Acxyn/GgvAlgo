using System;

namespace AcxynAPI.Model
{
    public class CsvData
    {
        public string GameName { get; set; }
        public string AvgUserRating { get; set; }
        public string NoOfUserRating { get; set; }
        public string DAU { get; set; }
        public string Installs { get; set; }
        public string Playtime { get; set; }
        public string D1Retention { get; set; }
        public string D7Retention { get; set; }
        public string D30Retention { get; set; }
        public string AvgSessionCount { get; set; }
        public string AvgSessionLength { get; set; }
        public string CurrentVisitScore { get; set; }
        public string AllTimePerformanceScore { get; set; }
    }

    public class GameDataDemo : CsvData
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
        public int ggv { get; set; }

    }
}
