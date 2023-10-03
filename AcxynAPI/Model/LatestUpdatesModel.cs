using System;

namespace AcxynAPI.Model
{
    public class LatestUpdatesModel
    {
        public string bannerUrl { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string redirectUrl { get; set; }
        public DateTime publishDate { get; set; }
    }
}