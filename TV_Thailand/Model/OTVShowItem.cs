using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TV_Thailand.Model
{
    class OTVShowItem
    {
        public string id { get; set; }
        public string nameTh { get; set; }
        public string nameEn { get; set; }
        public string thumbnail { get; set; }
        public string detail { get; set; }

        public string isOtv { get; set; }
        public string otvId { get; set; }
        public string otvApiName { get; set; }
    }
}
