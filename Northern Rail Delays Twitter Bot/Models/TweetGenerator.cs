using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using TweetSharp;

namespace Northern_Rail_Delays_Twitter_Bot
{
    class TweetGenerator
    {
        Database db = new Database();
        public List<jTrains.DelayedTrain> delayedNorthernTrains = new List<jTrains.DelayedTrain>();
        public static Dispatcher dispatcher;
        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "";
        private static string access_token_secret = "";
        private static TwitterService service = new TwitterService(customer_key,customer_key_secret, access_token, access_token_secret);
        public static RichTextBox outputTextBox;

        private void DeserializeJSON(List <string>StationCodes)
        {
            RESTclient rClient = new RESTclient();
            delayedNorthernTrains.Clear();

            foreach (var stationCode in StationCodes) //duplicate foreach loops with assigment of {0} and {1} flipped: redundant code so could be better implemented 
            {
                rClient.endPoint = string.Format("https://huxley.apphb.com/delays/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", stationCode, "liv");
                string strResponse = rClient.makeRequest();
                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                var filteredTrains = gottenjTrains.delayedTrains.Where(item => gottenjTrains.delayedTrains.Count(x => x.serviceID == item.serviceID) < 2);
                foreach (var train in filteredTrains)
                {
                    delayedNorthernTrains.Add(train);
                }
            }

            foreach (var stationCode in StationCodes)
            {
                rClient.endPoint = string.Format("https://huxley.apphb.com/delays/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", "liv", stationCode);
                string strResponse = rClient.makeRequest();
                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                var filteredTrains = gottenjTrains.delayedTrains.Where(item => gottenjTrains.delayedTrains.Count(x => x.serviceID == item.serviceID) < 2);
                foreach (var train in filteredTrains)
                {
                    delayedNorthernTrains.Add(train);
                }
            }

        }

        public void FillTrainObj()
        {
            string[] _stationCodes = { "wgn", "pre", "mcv", "bpn","ncl","wbq","lpy","mco","mia" };
            List <string> _stationCodesList = new List<string>();
            _stationCodesList.AddRange(_stationCodes);
            DeserializeJSON(_stationCodesList);
            //InitMockConnection();
        }

        public string delayedTrainCheck()
        {
            string returnStr="";
            bool detectedCancellation = false;
            int numOfCancellations = 0;

            if (delayedNorthernTrains.Count != 0)
            {
                foreach (var train in delayedNorthernTrains)
                {
                    if (train.cancelReason != null && db.CheckServiceID(train.serviceID) == 0)
                    {
                        db.SaveServiceIDs(train.serviceID.ToString());
                        int _ApolTicketNum = db.GetApologyTicketNum();
                        int newApolTicketNum = _ApolTicketNum + 159;
                        db.SaveApologyTicketNum(newApolTicketNum);

                        detectedCancellation = true;
                        numOfCancellations += 1;

                        string msg = string.Format("\rThe {0} service from {1} to {2} was cancelled. {3}. New Apology Ticket Number: {4}", train.previousCallingPoints[0].callingPoint[0].st.ToString(), 
                            train.origin[0].locationName, train.destination[0].locationName, train.cancelReason.ToString(), newApolTicketNum);
                        returnStr += msg;
                    }

                    else
                    {
                        string msg = string.Format("\rThere are delays, but no new cancelled trains.\rDelay count: " + delayedNorthernTrains.Count);
                        returnStr = msg;
                    }

                    returnStr += numOfCancellations.ToString();
                }
            }

            else if (detectedCancellation == true)
            {

                int oldTotalDelays = db.GetTotalDelaysNum();
                db.SaveTotalDelaysNum(oldTotalDelays + numOfCancellations);
                int newTotalDelays = db.GetTotalDelaysNum();

                string tweet = string.Format("This bot has detected {0} new cancellations from Northern Rail trains going to and from Liverpool Limestreet! If all 159 seats on each train was filled they would owe" +
                    " {1} new apology slips based on Japanese train operating procedure. This bot has detected {2} total delays attributing to {3} apology slips to Northern Rail customers.", numOfCancellations, (numOfCancellations * 159), newTotalDelays, db.GetApologyTicketNum());
                SendTweet(tweet, outputTextBox);
            }
            else
            {
                returnStr = string.Format("\rNo delayed or cancelled trains at {0}. Apology ticket num: {1}", DateTime.Now, db.GetApologyTicketNum().ToString());
            }
                
            return returnStr;
        }

        public string OriginDate()
        {
            return db.GetOriginDate();
        }


        public void InitMockConnection() //this method reads JSON from a text file rather than from a httprequest. Usual for offline development and for always having a collection member in the trainsDelayed list
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader("MockJSON.txt"))
            {
                string json = r.ReadToEnd();
                delayedNorthernTrains.Clear();
                
                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(json);
                //IEnumerable<jTrains.DelayedTrain> newDelayedTrains = gottenjTrains.delayedTrains.Where(x => !delayedNorthernTrains.Any(y => x.serviceID == y.serviceID)).ToList();
                var filteredTrains = gottenjTrains.delayedTrains.Where(item => gottenjTrains.delayedTrains.Count(x => x.serviceID == item.serviceID) < 2);
                foreach (var train in filteredTrains)
                {
                    delayedNorthernTrains.Add(train);
                }
            }
        }

        #region TwitterAPIMethods
        public void SendTweet(string _status, RichTextBox OutputTextbox)
        {
            service.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dispatcher.Invoke(() =>
                    {
                        OutputTextbox.SelectionBrush = Brushes.Green;
                        OutputTextbox.AppendText(string.Format("\r\n*****************************\rTweet sent: {0} \r*****************************\r", _status));
                    });
                }

                else
                {
                    dispatcher.Invoke(() =>
                    {
                        OutputTextbox.SelectionBrush = Brushes.Red;
                        OutputTextbox.AppendText("\r\nError sending tweet, statuscode:  " + response.StatusCode);
                    });
                }
            });
        }
        #endregion TwutterAPIMethods
    }
}
