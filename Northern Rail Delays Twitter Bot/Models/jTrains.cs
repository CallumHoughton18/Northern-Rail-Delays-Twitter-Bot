using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northern_Rail_Delays_Twitter_Bot.Models
{
    class jTrains
    {
        public List<TrainService> trainServices { get; set; }
        public object busServices { get; set; }
        public object ferryServices { get; set; }
        public DateTime generatedAt { get; set; }
        public string locationName { get; set; }
        public string crs { get; set; }
        public string filterLocationName { get; set; }
        public string filtercrs { get; set; }
        public int filterType { get; set; }
        public List<NrccMessage> nrccMessages { get; set; }
        public bool platformAvailable { get; set; }
        public bool areServicesAvailable { get; set; }

        public class Origin
        {
            public string locationName { get; set; }
            public string crs { get; set; }
            public object via { get; set; }
            public object futureChangeTo { get; set; }
            public bool assocIsCancelled { get; set; }
        }

        public class Destination
        {
            public string locationName { get; set; }
            public string crs { get; set; }
            public object via { get; set; }
            public object futureChangeTo { get; set; }
            public bool assocIsCancelled { get; set; }
        }

        public class TrainService
        {
            public List<Origin> origin { get; set; }
            public List<Destination> destination { get; set; }
            public string currentOrigins { get; set; }
            public string currentDestinations { get; set; }
            public string rsid { get; set; }
            public string sta { get; set; }
            public string eta { get; set; }
            public string std { get; set; }
            public string etd { get; set; }
            public string platform { get; set; }
            public string @operator { get; set; }
            public string operatorCode { get; set; }
            public bool isCircularRoute { get; set; }
            public bool isCancelled { get; set; }
            public bool filterLocationCancelled { get; set; }
            public int serviceType { get; set; }
            public int length { get; set; }
            public bool detachFront { get; set; }
            public bool isReverseFormation { get; set; }
            public object cancelReason { get; set; }
            public object delayReason { get; set; }
            public string serviceID { get; set; }
            public string serviceIdPercentEncoded { get; set; }
            public string serviceIdGuid { get; set; }
            public string serviceIdUrlSafe { get; set; }
            public object adhocAlerts { get; set; }
        }

        public class NrccMessage
        {
            public string value { get; set; }
        }
    }
}
