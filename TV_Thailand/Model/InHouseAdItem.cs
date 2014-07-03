using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TV_Thailand.Class
{
    class InHouseAdItem
    {
        public string name { get; set; }
        public string url { get; set; }
        public string time { get; set; }

        public InHouseAdItem(JToken ad)
        {
            this.name = ad["name"].Value<string>();
            this.url = ad["url"].Value<string>();
            this.time = ad["time"].Value<string>();
        }
    }
}
