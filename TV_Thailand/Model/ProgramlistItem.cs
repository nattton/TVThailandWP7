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
    }
}
