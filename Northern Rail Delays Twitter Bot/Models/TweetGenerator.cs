using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northern_Rail_Delays_Twitter_Bot
{
    class TweetGenerator
    {
        public string totTrainsDelayed;
        public string reasonForDelay;
        jTrains _jTrains; 


        public void deserializeJSON()
        {
            RESTclient rClient = new RESTclient();
            rClient.endPoint = "https://huxley.apphb.com/delays/wgn/from/liv/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1";
            string strResponse = rClient.makeRequest();
            _jTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
            totTrainsDelayed = _jTrains.totalTrainsDelayed.ToString();
        }


    }
}
