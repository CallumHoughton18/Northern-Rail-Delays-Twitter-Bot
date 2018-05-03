using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        RESTclient rClient = new RESTclient();
        public List<jTrains.DelayedTrain> delayedNorthernTrains = new List<jTrains.DelayedTrain>();
        public static Dispatcher dispatcher;
        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "-";
        private static string access_token_secret = "";
        private static TwitterService service = new TwitterService(customer_key,customer_key_secret, access_token, access_token_secret);
        public static RichTextBox outputTextBox;

        #region JSON manipulation methods

        private void DeserializeJSON(List <string>StationCodes)
        {
            try
            {
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

                foreach (var stationCode in StationCodes) //duplicate foreach loop gets trains going FROM the stations in station code TO liverpool limestreet
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
            catch
            {
                if (rClient.CheckConnection("https://huxley.apphb.com/all/wgn/from/liv/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1") == false)
                {
                    if(MessageBox.Show("Cannot connect to Huxley api! Exiting application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                    }

                }

                else
                {
                    MessageBox.Show("Problems deserializing JSON, try an application restart", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        public void FillTrainObj()
        {
            string[] _stationCodes = { "wgn", "pre", "mcv", "bpn","wbq","lpy","mco","mia","wbq","wac" };
            List <string> _stationCodesList = new List<string>();
            _stationCodesList.AddRange(_stationCodes);
            DeserializeJSON(_stationCodesList);
            //InitMockConnection();
        }

        public string DelayedTrainCheck()
        {
            string returnStr="";

            if (delayedNorthernTrains.Count != 0)
            {
                foreach (var train in delayedNorthernTrains)
                {
                    if (train.eta.ToUpper() == "CANCELLED" && db.CheckServiceID(train.serviceID) == 0) //if train item in the list is marked as cancelled and is not saved in the DB then the msg string will be tweeted.
                    {
                        db.SaveServiceIDs(train.serviceID.ToString());

                        int _ApolTicketNum = db.GetApologyTicketNum();
                        int newApolTicketNum = _ApolTicketNum + 159;
                        db.SaveApologyTicketNum(newApolTicketNum);

                        int oldTotCancellations = db.GetTotalCancelsNum();
                        int newCancellations = oldTotCancellations + 1;
                        db.SaveTotalCancelsNum(newCancellations);

                        string msg = string.Format("\rThe Northern Rail {0} service from {1} to {2} was cancelled. You owe 159 new apology slips if all the seats where filled. @northernassist", train.previousCallingPoints[0].callingPoint[0].st.ToString(), 
                            train.origin[0].locationName, train.destination[0].locationName, DateTime.Now.ToString());
                        SendTweet(msg, outputTextBox);
                        returnStr += msg;

                    }

                    else
                    {
                        string msg = string.Format("\rThere are delays, but no new cancelled trains.\rDelay count: " + delayedNorthernTrains.Count);
                        returnStr = msg;
                    }
                }
            }

            else
            {
                returnStr = string.Format("\rNo delayed or cancelled trains at {0}.\rApology ticket Total: {1}\rTotal Cancellations: {2}", DateTime.Now, db.GetApologyTicketNum().ToString(), db.GetTotalCancelsNum());
            }
                
            return returnStr;
        }
        public void InitMockConnection() //this method reads JSON from a text file rather than from a httprequest. Usual for offline development and for always having a collection member in the trainsDelayed list
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader("MockJSON.txt"))
            {
                string json = r.ReadToEnd();
                delayedNorthernTrains.Clear();

                jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(json);
                var filteredTrains = gottenjTrains.delayedTrains.Where(item => gottenjTrains.delayedTrains.Count(x => x.serviceID == item.serviceID) < 2);
                foreach (var train in filteredTrains)
                {
                    delayedNorthernTrains.Add(train);
                }
            }
        }

        #endregion

        #region Database ONLY interaction methods

        public string OriginDate()
        {
            return db.GetOriginDate();
        }

        public string GetTotalDelaysAndApols()
        {
            return string.Format("\rTotal Cancellations: {0} \rTotal Apology Tickets: {1}", db.GetTotalCancelsNum(), db.GetApologyTicketNum());
        }

        public void CheckCurrentDate()
        {
            if (db.GetSavedDate() != DateTime.Now.ToShortDateString())
            {
                db.DeleteAllServiceIDs();
                db.SaveCurrentDate();
            }

            else
            {

            }
        }

        #endregion

        #region Tweet posting ONLY methods

        public void TweetTotalDelaysAndApologyNum()
        {
            string message = string.Format("Since {0} this bot has detected {1} cancellations resulting in {2} apology slips.", OriginDate(), db.GetTotalCancelsNum(), db.GetApologyTicketNum());
            SendTweet(message, outputTextBox);
        }

        #endregion


        #region TwitterAPIMethods
        public void SendTweet(string _status, RichTextBox OutputTextbox)
        {
            service.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK && _status != "")
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
