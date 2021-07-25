using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            var filepath = "mdata" + System.IO.Path.DirectorySeparatorChar + "photos" + System.IO.Path.DirectorySeparatorChar + path;
            if(System.IO.File.Exists(filepath))
            {
                DateTimeOffset dateTimeOffset = System.IO.File.GetLastWriteTimeUtc(filepath);
                return File(System.IO.File.OpenRead(filepath), "image/jpeg", dateTimeOffset,
                    new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\""+ETagGenerator.GenerateETag(Encoding.UTF8.GetBytes(filepath)) + "\""));
            }
            else
            {
                return NotFound();
            }
        }

    }
    static class ETagGenerator
    {
        public static string GetETag(string key, byte[] contentBytes)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var combinedBytes = Combine(keyBytes, contentBytes);

            return GenerateETag(combinedBytes);
        }

        public static string GenerateETag(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return Pages.WorkDetailModel.hex(hash);
            }
        }

        public static byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }
}
