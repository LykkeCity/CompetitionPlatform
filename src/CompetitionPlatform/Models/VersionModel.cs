using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models
{
    public class VersionModel
    {
        public string Version { get; set; }
    }

    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
    }
}
