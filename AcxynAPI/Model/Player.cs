using System;

namespace AcxynAPI.Model
{
    public class PlayerLoginLogout
    {
        public string Username { get; set; }
        public string ApiKey { get; set; }
    }

    public class PLayerIAP
    {
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }

        public decimal Amount { get; set; }
        public string ApiKey { get; set; }
    }
}
