using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class Config
    {
        IConfigurationRoot cfg { get; set; }
        public Config() => cfg = new ConfigurationBuilder().AddJsonFile("cfg.json").Build();
        public string this[string str] => cfg[str];
    }
}
