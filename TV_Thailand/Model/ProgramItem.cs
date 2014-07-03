using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TV_Thailand
{
    public class ProgramItem
    {
        public string program_id {get; set;}
        public string title { get; set; }
        public string thumbnail { get; set; }
        public string description { get; set; }
        public bool is_otv { get; set; }
        public string otv_id { get; set; }
        public string otv_api_name { get; set; }

        public ProgramItem(JToken program)
        {
            this.program_id = program["id"].Value<string>();
            this.title = program["title"].Value<string>();
            this.thumbnail = program["thumbnail"].Value<string>();
            this.description = program["description"].Value<string>();
            this.is_otv = "1".Equals(program["is_otv"].Value<string>());
            this.otv_id = program["otv_id"].Value<string>();
            this.otv_api_name = program["otv_api_name"].Value<string>();
        }
    }
}
