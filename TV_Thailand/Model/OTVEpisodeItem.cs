using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TV_Thailand.Model
{
    class OTVEpisodeItem
    {
        public string id { get; set; }
        public string nameTh { get; set; }
        public string detail { get; set; }
        public string thumbnail { get; set; }
        public string date { get; set; }
        public List<OTVPartItem> parts { get; set; }
    }
}
