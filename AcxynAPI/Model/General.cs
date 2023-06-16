using System;

namespace AcxynAPI.Model
{
    public class Response
    {
        public bool Success { get; set; } = true;
        public string ErrorCode { get; set; }
        public string ResponseJson { get; set; }
        public string Message { get; set; }
    }

    public class NftStorageResp
    {
        public bool ok { get; set; }
        public NftStorageRespValue value { get; set; }
    }

    public class NftStorageRespValue
    {
        public string cid { get; set; }
    }

    public class Example
    {
        public string NFTAddress { get; set; }
        public int Value { get; set; }
    }
}
