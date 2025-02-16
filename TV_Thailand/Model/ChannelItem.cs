﻿using Newtonsoft.Json.Linq;
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
    public class ChannelItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string thumbnail { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public bool hasShow { get; set; }

        public ChannelItem ()
        {

        }

        public ChannelItem (JToken channel)
        {
            this.id = channel["id"].Value<string>();
            this.title = channel["title"].Value<string>();
            this.description = (channel["description"] != null) ? channel["description"].Value<string>(): "";
            this.thumbnail = (channel["thumbnail"] != null) ? channel["thumbnail"].Value<string>() : "";
            this.url = (channel["url"] != null) ? channel["url"].Value<string>() : "";
            this.hasShow = ("1".Equals(channel["has_show"].Value<string>()));
        }
    }
}
