using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HappyFeet
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Data
    {
        [JsonProperty]
        public string name { get; set; }

        [JsonProperty]
        public string data { get; set; }
        
        public Data()
        {
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static Data Deserialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<Data>(jsonString);
        }
    }
}
