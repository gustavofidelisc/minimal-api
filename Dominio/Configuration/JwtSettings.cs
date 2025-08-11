using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.Configuration
{
    public struct JwtSettings
    {
        public string Key { get; set; }
        public string Audience { get; set; }
    }
}