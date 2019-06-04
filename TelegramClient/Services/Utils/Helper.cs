using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramClient.Services.Utils
{
    public class Helper
    {
        public static string DeleteUrls(string str)
        {
            str = str + ' ';
            while (true)
            {
                var indx = str.IndexOf("http");
                if (indx > 0)
                {
                    var http = str.Substring(indx);
                    var indxSpace = http.IndexOf(' ');
                    var afterUrl = http.Substring(indxSpace);
                    str = str.Substring(0, indx) + afterUrl;
                }
                else break;
            }

            return str;
        }
    }

    public static class ObjectExtension
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
