using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LipstickTaggerWebApplication
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiDataController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetImg([FromQuery]string path)
        {
            return File(System.IO.File.OpenRead("mdata\\photos\\" + path), "image/jpeg");
        }

    }
}
