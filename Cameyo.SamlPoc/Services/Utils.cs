using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace Cameyo.SamlPoc.Services
{
    public static class Utils
    {
        public static string GetEmailDomain(string email)
        {
            MailAddress address = new MailAddress(email);
            return address.Host;
        }

        public static string SerializeToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                //DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                //Error = (sender, args) => args.ErrorContext.Handled = true,
            });
        }

        public static T DeserializeFromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static void WriteTextToFile(string path, string data)
        {
            using (var streamWriter = new StreamWriter(path, false))
            {
                streamWriter.Write(data);
            }
        }

        public static string ReadTextFromFile(string path)
        {
            string data;

            using (var streamReader = new StreamReader(path, Encoding.UTF8))
            {
                data = streamReader.ReadToEnd();
            }

            return data;
        }
    }
}