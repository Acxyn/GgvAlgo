using System;

namespace AcxynAPI.Model
{
    public class NFT
    {
        public string MinterAddress { get; set; }
        public string ImageBase64 { get; set; }
        public string MetadataJson { get; set; }
        public string ApiKey { get; set; }
    }

    public class NFTResp
    {
        public string CID { get; set; }
        public string ImageUrl { get; set; }
        public string TransactionAddress { get; set; }
    }
}
