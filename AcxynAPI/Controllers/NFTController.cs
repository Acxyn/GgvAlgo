using AcxynAPI.Common;
using AcxynAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Netezos.Contracts;
using Netezos.Encoding;
using Netezos.Forging;
using Netezos.Forging.Models;
using Netezos.Keys;
using Netezos.Rpc;
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
    public class NFTController : ControllerBase
    {
        private readonly ILogger<NFTController> _logger;
        private readonly IConfiguration _configuration;

        public NFTController(ILogger<NFTController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public Response Post(NFT nft)
        {
            mintNFTAsync();
            Response response = new Response();
            return response;
            //validation 
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@api_key", nft.ApiKey);

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
                var htmlUrl = "https://api.nft.storage/upload";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(htmlUrl);
                httpWebRequest.ContentType = "image/*";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkaWQ6ZXRocjoweENCMDE5QmNkZjQ2N0ZFMEE4MTkxY0Y5ZGU2NUQwQTA5QUUzNzk4RTQiLCJpc3MiOiJuZnQtc3RvcmFnZSIsImlhdCI6MTY2NjAwMjg1MTMzNiwibmFtZSI6ImRlbW8ifQ.Ed_4hrp_j_DjEDX3ON-ai7Zd9V28ibZlQsxk87tB4Z4");

                //int contentLength = fileUpload.PostedFile.ContentLength;
                //byte[] data = new byte[contentLength];
                //fileUpload.PostedFile.InputStream.Read(data, 0, contentLength);

                //byte[] data = Encoding.ASCII.GetBytes(nft.Image);
                byte[] data = Convert.FromBase64String(nft.ImageBase64);
                //byte[] data = System.IO.File.ReadAllBytes(nft.Image);

                using (Stream postStream = httpWebRequest.GetRequestStream())
                {
                    // Send the data.
                    postStream.Write(data, 0, data.Length);
                    postStream.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var htmlResult = streamReader.ReadToEnd();
                    var resultJson = JsonSerializer.Deserialize<NftStorageResp>(htmlResult);

                

                    NFTResp NFTResp = new NFTResp();
                    NFTResp.CID = resultJson.value.cid;
                    NFTResp.ImageUrl = "https://" + resultJson.value.cid + ".ipfs.nftstorage.link";

                    response.ResponseJson = JsonSerializer.Serialize(NFTResp);
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

        private async Task<string> mintNFTAsync()
        {
            // generate new key
            //var key = new Key();

            //// or use existing one
            //var key = Key.FromBase58("edskRpm7zDTNTLonGJe4Bv4JH2rsvpJ3gzWaUnMGJGfvoaWoJpfP573X4as1RUCMfZmzZktbrSuVv1vzFdwxnKCZV1wjken19c");

            //// use this address to receive some tez
            //var address = key.PubKey.Address; // tz1SauKgPRsTSuQRWzJA262QR8cKdw1d9pyK

            //using var rpc = new TezosRpc("https://mainnet-tezos.giganode.io/");

            //// get a head block
            //var head = await rpc.Blocks.Head.Hash.GetAsync<string>();

            //// get account's counter
            //var counter = await rpc.Blocks.Head.Context.Contracts[address].Counter.GetAsync<int>();
            try
            {
                var rpc = new TezosRpc("https://rpc.tzkt.io/ghostnet/");
                var key = Key.FromBase58("edskRpm7zDTNTLonGJe4Bv4JH2rsvpJ3gzWaUnMGJGfvoaWoJpfP573X4as1RUCMfZmzZktbrSuVv1vzFdwxnKCZV1wjken19c");
                var address = key.PubKey.Address;

                var recepient = Key.FromBase58("edskS4QKfY7DVqUCFJGP5wwq1LGA7H4evcSNHgeDADcNwpew6tbQW2HHyDuSj5x8SGManiV976rBEYgCBv8W4fbkxop8wY5Vcb");

                var FA2 = "KT1B1iwW186QqX7RgLn1aqoyE3msLr38SAJj";
                //var FA2 = "KT1BST3ARHB2tBBbp8FnMbDyQwnhaZXtEGR3";
                // get a head block
                var head = await rpc.Blocks.Head.Hash.GetAsync<string>();

                // get account's counter
                var counter = await rpc.Blocks.Head.Context.Contracts[address].Counter.GetAsync<int>();

                // get the script of the contract from the RPC
                var script = await rpc.Blocks.Head.Context.Contracts[FA2].Script.GetAsync();

                // Deserialize the script code JSON string to the `IMicheline` object
                var code = Micheline.FromJson(script.code);

                var cs = new ContractScript(code);
               

               var schemaString = cs.Entrypoints["mint"].Humanize();
                Console.WriteLine(schemaString);

                //var param = cs.BuildParameter(
                //    "transfer",
                //    new List<object>
                //    {
                //        new
                //        {
                //            from_ = key.PubKey.Address,
                //            txs = new List<object>
                //            {
                //                new
                //                {
                //                    to_ = recepient.PubKey.Address,
                //                    token_id = 0,
                //                    amount = "1"
                //                }
                //            }
                //        }
                //    });

                //var tx = new TransactionContent
                //{
                //    Source = key.PubKey.Address,
                //    Counter = ++counter,
                //    GasLimit = 100_000,
                //    Fee = 100_000,
                //    StorageLimit = 257,
                //    Destination = FA2,
                //    Parameters = new Parameters
                //    {
                //        Entrypoint = "transfer",
                //        Value = param
                //    }
                //};
                var tokenInfo = new
                {
                    decimals = "0",
                    name = "test",
                    symbol = "Acxyn"
                };
                string x = JsonSerializer.Serialize(tokenInfo);
                var param = cs.BuildParameter(
                  "mint",
                   new
                   {
                       owner = key.PubKey.Address,
                       token_info = "test2"
                   });

                var tx = new TransactionContent
                {
                    Source = key.PubKey.Address,
                    Counter = ++counter,
                    GasLimit = 100_000,
                    Fee = 100_000,
                    StorageLimit = 257,
                    Destination = FA2,
                    Parameters = new Parameters
                    {
                        Entrypoint = "mint",
                        Value = param
                    }
                };

                var opBytes = await new LocalForge().ForgeOperationAsync(head, tx);
                var opSig = key.SignOperation(opBytes);

                var opHash = await rpc.Inject.Operation.PostAsync(opBytes.Concat((byte[])opSig));

                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
          
        }
    }
}
