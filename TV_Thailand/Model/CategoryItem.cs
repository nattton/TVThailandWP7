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
    public class CategoryItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string thumbnail { get; set; }

        public CategoryItem ()
        {

        }

        public CategoryItem (JToken category)
        {
            this.id = category["id"].Value<string>();
            this.title = category["title"].Value<string>();
            this.thumbnail = category["thumbnail"].Value<string>();
        }
    }
}
