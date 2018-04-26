using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northern_Rail_Delays_Twitter_Bot.Models
{
    class jTrains
    {
        public string generatedAt { get; set; }
        public string locationName { get; set; }
        public string crs { get; set; }
        public string filterLocationName { get; set; }
        public string filtercrs { get; set; }
        public bool delays { get; set; }
        public double totalTrainsDelayed { get; set; }
        public double totalDelayMinutes { get; set; }
        public double totalTrains { get; set; }
        public ICollection<string> delayedTrains { get; set; }

    }
}
