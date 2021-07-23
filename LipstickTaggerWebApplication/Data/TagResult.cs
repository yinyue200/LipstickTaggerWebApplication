using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LipstickTaggerWebApplication.Data
{
    public class TagResult
    {
        public string Path { get; set; }
        public List<string> PhotosTags { get; set; }
        public string LastEditBy { get; set; }
    }
}
