using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Northern_Rail_Delays_Twitter_Bot
{
    class TweetGenerator
    {
        public string totTrainsDelayed;
        public string reasonForDelay;
        jTrains _jTrains; 


        public void deserializeJSON()
        {
            /*RESTclient rClient = new RESTclient();
            rClient.endPoint = "https://huxley.apphb.com/delays/wgn/from/liv/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1";
            string strResponse = rClient.makeRequest();
            _jTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);*/

            InitMockConnection();

            totTrainsDelayed = _jTrains.totalTrainsDelayed.ToString();
        }

        public string delayedTrainCheck()
        {
            string returnStr="";
            if (_jTrains.delayedTrains.Count == 0)
            {
                returnStr="No trains are delayed";
            }

            else
            {
                foreach (var train in _jTrains.delayedTrains)
                {
                    string  msg = string.Format("\nThe {0} service from {1} to {2} was cancelled. {3}", train.previousCallingPoints[0].callingPoint[0].st.ToString(), train.origin[0].locationName, train.destination[0].locationName, train.cancelReason.ToString());
                    returnStr += msg;
                }
                
            }

            return returnStr;
        }

        public void InitMockConnection() //this method reads JSON from a text file rather than from a httprequest. Usual for offline development and for always having a collection member in the trainsDelayed list
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader("MockJSON.txt"))
            {
                string json = r.ReadToEnd();
                _jTrains = JsonConvert.DeserializeObject<jTrains>(json);
            }
        }

        public void SaveServiceID()
        {
            //save service ID to datafile
        }




    }
}
