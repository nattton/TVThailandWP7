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
    public class EpItem
    {
        public string videoKey {get; set;}
        public string epname { get; set; }
        public string thumbnail { get; set; }

        public EpItem ()
        {

        }

        public EpItem (int i, int length, string videoKey, string src_type)
        {
            this.epname = "ตอนที่ " + (i + 1).ToString() + " / " + length.ToString();
            this.videoKey = videoKey;
            this.thumbnail = Utility.Instance.videoThumbnail(videoKey, src_type);
        }
    }
}
