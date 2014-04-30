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
    }
}
