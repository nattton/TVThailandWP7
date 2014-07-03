using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TV_Thailand.Model
{
    class RadioItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string thumbnail { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string category { get; set; }

        public RadioItem(JToken radio)
        {
            this.id = radio["id"].Value<string>();
            this.title = radio["title"].Value<string>();
            this.description = radio["description"].Value<string>();
            this.thumbnail = (radio["thumbnail"] != null) ? radio["thumbnail"].Value<string>() : "";
            this.url = (radio["url"] != null) ? radio["url"].Value<string>() : "";
            this.category = radio["category"].Value<string>();
        }
    }
}
