using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northern_Rail_Delays_Twitter_Bot.Models
{
    class jTrains
    {
        public DateTime generatedAt { get; set; }
        public string locationName { get; set; }
        public string crs { get; set; }
        public string filterLocationName { get; set; }
        public string filtercrs { get; set; }
        public bool delays { get; set; }
        public int totalTrainsDelayed { get; set; }
        public int totalDelayMinutes { get; set; }
        public int totalTrains { get; set; }
        public List<DelayedTrain> delayedTrains { get; set; }
        public class CallingPoint
        {
            public string locationName { get; set; }
            public string crs { get; set; }
            public string st { get; set; }
            public string et { get; set; }
            public object at { get; set; }
            public bool isCancelled { get; set; }
            public int length { get; set; }
            public bool detachFront { get; set; }
            public object adhocAlerts { get; set; }
        }

        public class PreviousCallingPoint
        {
            public List<CallingPoint> callingPoint { get; set; }
            public int serviceType { get; set; }
            public bool serviceChangeRequired { get; set; }
            public bool assocIsCancelled { get; set; }
        }

        public class CallingPoint2
        {
            public string locationName { get; set; }
            public string crs { get; set; }
            public string st { get; set; }
            public string et { get; set; }
            public object at { get; set; }
            public bool isCancelled { get; set; }
            public int length { get; set; }
            public bool detachFront { get; set; }
            public object adhocAlerts { get; set; }
        }

        public class SubsequentCallingPoint
        {
            public List<CallingPoint2> callingPoint { get; set; }
            public int serviceType { get; set; }
            public bool serviceChangeRequired { get; set; }
            public bool assocIsCancelled { get; set; }
        }

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

        public class DelayedTrain
        {
            public List<PreviousCallingPoint> previousCallingPoints { get; set; }
            public List<SubsequentCallingPoint> subsequentCallingPoints { get; set; }
            public List<Origin> origin { get; set; }
            public List<Destination> destination { get; set; }
            public object currentOrigins { get; set; }
            public object currentDestinations { get; set; }
            public string rsid { get; set; }
            public string sta { get; set; }
            public string eta { get; set; }
            public string std { get; set; }
            public string etd { get; set; }
            public object platform { get; set; }
            public string @operator { get; set; }
            public string operatorCode { get; set; }
            public bool isCircularRoute { get; set; }
            public bool isCancelled { get; set; }
            public bool filterLocationCancelled { get; set; }
            public int serviceType { get; set; }
            public int length { get; set; }
            public bool detachFront { get; set; }
            public bool isReverseFormation { get; set; }
            public string cancelReason { get; set; }
            public object delayReason { get; set; }
            public string serviceID { get; set; }
            public string serviceIdPercentEncoded { get; set; }
            public string serviceIdGuid { get; set; }
            public string serviceIdUrlSafe { get; set; }
            public object adhocAlerts { get; set; }
        }

    }
}
