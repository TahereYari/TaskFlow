using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Common
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int ExpireDays { get; set; }
    }
}
