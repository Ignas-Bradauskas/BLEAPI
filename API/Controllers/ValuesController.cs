using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/devices")]
    [ApiController]
    [Produces("application/json")]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            var a = JsonConvert.SerializeObject(BLEConsoleApp.DeviceLocatorService.Poll());
            return Ok(BLEConsoleApp.DeviceLocatorService.Poll());
        }

    }
}
