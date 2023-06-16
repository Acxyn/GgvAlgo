using System;

namespace AcxynAPI.Model
{
    public class Game
    {
        public int GameId { get; set; }
        public decimal BucketSize { get; set; }
        public decimal NetworkValue { get; set; }
        public decimal TransactionVolume { get; set; }
        public int IntegratedFunction { get; set; }
        public int AvgPlayerRetention { get; set; }
        public int AvgPlayerCount { get; set; }
        public int AvgPlayTime { get; set; }
        public int AvgAdRev { get; set; }
        public int AvgRPU { get; set; }
        public int GGV { get; set; }
    }
}