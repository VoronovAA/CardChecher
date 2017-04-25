using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardChecher
{

    class Config  
    {
        public ConfigJSONWrapper item { get; set; }

        public Config()
        {
            if (File.Exists(@"config.json"))
            {
                
              this.item = JsonConvert.DeserializeObject<ConfigJSONWrapper>(new StreamReader(File.OpenRead(@"config.json")).ReadToEnd());
                
            }
            else
            {
                throw new System.IO.FileNotFoundException("config.json not found");
            }
        }

        public override string ToString()
        {
            return " username " + item.username + " token " + item.token + " filePath " + item.fileDir;
        }
    }

    public class ConfigJSONWrapper
    {
        public String username { get; set; }
        public String password { get; set; }
        public String token { get; set; }
        public String fileDir { get; set; }
        public String client_id { get; set; }
        public String client_secret { get; set; }
        public String domain { get; set; }
        public String endpoint { get; set; }
        public int timeout { get; set; }
    }    
}
