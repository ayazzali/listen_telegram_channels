using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class Config
    {
        IConfiguration cfg { get; set; }
        ILogger logger;
        public Config(IConfiguration cf, ILogger<Config> log) { cfg = cf; logger = log; }
        public string this[string str]
        {
            get
            {
                var s = cfg[str];
                if (s == null || s.Contains(str))
                    logger.LogError(str + " is " + s);
                return s;
            }
        }
    }
}
