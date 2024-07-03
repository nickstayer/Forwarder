using MailKit.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forwarder
{
    public class Settings
    {
        public string BodyTextDefault { get; set; }
        public  int PeriodMinutes { get; set; }
        public  string User { get; set; }
        public  string Password { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public  string ImapServer { get; set; }
        public  int ImapPort { get; set; }
        public SecureSocketOptions Options { get; set; }
        public bool UseSsl { get; set; }
        public bool UsernameWithoutDog { get; set; }
        public const string Formatt = "Пересылка;Адрес1,Адрес2;Тема";


        public static Settings? DeserializeSettings(string fileJson)
        {
            if (!File.Exists(fileJson))
            {
                throw new Exception($"{Consts.NO_SETTINGS_FILE}: {fileJson}");
            }
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileJson));
            return settings;
        }

        public void SerializeSettings(string fileJson)
        {
            if (!File.Exists(fileJson))
            {
                var fileStream = File.Create(fileJson);
                fileStream.Dispose();
            }
            File.WriteAllText(fileJson, JsonConvert.SerializeObject(this));
        }
    }
}
