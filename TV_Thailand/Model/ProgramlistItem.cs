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
    public class ProgramlistItem
    {
        public string programlist_id { get; set; }
        public string name { get; set; }
        public string youtube_encrypt { get; set; }
        public string src_type { get; set; }
        public string date { get; set; }
        public string count { get; set; }
        public string password { get; set; }
        
        public ProgramlistItem (JToken programlist)
        {
            this.programlist_id = programlist["id"].Value<string>();
            string epname = (programlist["title"].Value<string>().Equals("")) ? "" : " - " + programlist["title"].Value<string>();
            this.name = "ตอนที่ " + programlist["ep"].Value<string>() + epname;
            this.youtube_encrypt = programlist["video_encrypt"].Value<string>();
            this.src_type = programlist["src_type"].Value<string>();
            this.date = "ออกอากาศ " + programlist["date"].Value<string>();
            int count = Convert.ToInt32(programlist["view_count"].Value<string>());
            this.count = ((count == 0) ? "0" : count.ToString("#,###")) + " Views";
            this.password = programlist["pwd"].Value<string>();
        }
    }
}
