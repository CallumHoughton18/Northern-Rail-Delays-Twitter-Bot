﻿using Newtonsoft.Json;
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
        Database db = new Database();
        public List<jTrains.DelayedTrain> delayedNorthernTrains = new List<jTrains.DelayedTrain>();

        private void DeserializeJSON(List <string>StationCodes)
        {
            RESTclient rClient = new RESTclient();

            foreach (var stationCode in StationCodes) //duplicate foreach loops with assigment of {0} and {1} flipped: redundant code so could be better implemented 
            {
                rClient.endPoint = string.Format("https://huxley.apphb.com/delays/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", stationCode, "liv");
                string strResponse = rClient.makeRequest();
                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                //_jTrains.totalTrainsDelayed += newjTrains.totalTrainsDelayed; //need to check service IDs
                IEnumerable<jTrains.DelayedTrain> newDelayedTrains = gottenjTrains.delayedTrains.Where(x => !delayedNorthernTrains.Any(y => x.serviceID == y.serviceID)).ToList();
                foreach (var train in newDelayedTrains)
                {
                    delayedNorthernTrains.AddRange(newDelayedTrains);
                }
            }

            foreach (var stationCode in StationCodes)
            {
                rClient.endPoint = string.Format("https://huxley.apphb.com/delays/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", "liv", stationCode);
                string strResponse = rClient.makeRequest();
                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                //_jTrains.totalTrainsDelayed += newjTrains.totalTrainsDelayed; //need to check service IDs
                IEnumerable<jTrains.DelayedTrain> newDelayedTrains = gottenjTrains.delayedTrains.Where(x => !delayedNorthernTrains.Any(y => x.serviceID == y.serviceID)).ToList();
                foreach (var train in newDelayedTrains)
                {
                    delayedNorthernTrains.AddRange(newDelayedTrains);
                }
            }

        }

        public void FillTrainObj()
        {
            string[] _stationCodes = { "wgn", "pre", "mcv", "bpn","ncl" };
            List <string> _stationCodesList = new List<string>();
            _stationCodesList.AddRange(_stationCodes);
            DeserializeJSON(_stationCodesList);
            //InitMockConnection();
        }

        public string delayedTrainCheck()
        {
            string returnStr="";
            int _ApolTicketNum = db.GetApologyTicketNum();

            if (delayedNorthernTrains.Count != 0)
            {
                foreach (var train in delayedNorthernTrains)
                {
                    if (train.cancelReason != null && db.CheckServiceID() == 0)
                    {
                        db.SaveServiceIDs(train.serviceID.ToString());
                        int newApolTicketNum = _ApolTicketNum + 159;
                        db.SaveApologyTicketNum(newApolTicketNum);
                        string msg = string.Format("\nThe {0} service from {1} to {2} was cancelled. {3}. New Apology Ticket Number: {4}", train.previousCallingPoints[0].callingPoint[0].st.ToString(), train.origin[0].locationName, train.destination[0].locationName, train.cancelReason.ToString(), newApolTicketNum);
                        returnStr += msg;
                    }

                    else
                    {
                        
                        string msg = string.Format("\nThe {0} service from {1} to {2} was delayed", train.previousCallingPoints[0].callingPoint[0].st.ToString(), train.origin[0].locationName, train.destination[0].locationName);
                        returnStr = msg;
                    }
                }
            }
            else
            {
                returnStr = string.Format("No cancelled trains. Apology ticket num: {0}",_ApolTicketNum.ToString());
                
            }
                
            return returnStr;
        }


        public void InitMockConnection() //this method reads JSON from a text file rather than from a httprequest. Usual for offline development and for always having a collection member in the trainsDelayed list
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader("MockJSON.txt"))
            {
                string json = r.ReadToEnd();
                //_jTrains = JsonConvert.DeserializeObject<jTrains>(json);
            }
        }
    }
}
