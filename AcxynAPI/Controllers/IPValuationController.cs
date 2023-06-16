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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AcxynAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IPValuationController : ControllerBase
    {
        private readonly ILogger<IPValuationController> _logger;
        private readonly IConfiguration _configuration;

        public IPValuationController(ILogger<IPValuationController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

    }
}
