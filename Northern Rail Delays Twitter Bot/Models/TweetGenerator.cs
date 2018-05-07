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
        public List<jTrains.TrainService> NorthernTrains = new List<jTrains.TrainService>();
        public static Dispatcher dispatcher;
        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "";
        private static string access_token_secret = "";
        private static TwitterService service = new TwitterService(customer_key,customer_key_secret, access_token, access_token_secret);
        public static RichTextBox outputTextBox;

        #region JSON manipulation methods

        private void DeserializeJSON(List <string>StationCodes)
        {

            try
            {


                NorthernTrains.Clear();

                foreach (var stationCode in StationCodes) //duplicate foreach loops with assigment of {0} and {1} flipped: redundant code so could be better implemented 
                {
                    rClient.endPoint = string.Format("https://huxley.apphb.com/all/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", stationCode, "liv");
                    string strResponse = rClient.makeRequest();
                    jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                    if(gottenjTrains.trainServices != null)
                    {
                        var filteredTrains = gottenjTrains.trainServices.Where(item => gottenjTrains.trainServices.Count(x => x.serviceID == item.serviceID) < 2);
                        List<jTrains.TrainService> northernTrains = gottenjTrains.trainServices;

                        foreach (var train in filteredTrains)
                        {
                            if (train.@operator.ToUpper() == "NORTHERN")
                            {
                                NorthernTrains.Add(train);
                            }
                        }
                    }
                }

                foreach (var stationCode in StationCodes) //duplicate foreach loop gets trains going FROM the stations in station code TO liverpool limestreet
                {
                    rClient.endPoint = string.Format("https://huxley.apphb.com/all/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", "liv", stationCode);
                    string strResponse = rClient.makeRequest();
                    jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                    if (gottenjTrains.trainServices != null)
                    {
                        var filteredTrains = gottenjTrains.trainServices.Where(item => gottenjTrains.trainServices.Count(x => x.serviceID == item.serviceID) < 2);
                        List<jTrains.TrainService> northernTrains = gottenjTrains.trainServices;

                        foreach (var train in filteredTrains)
                        {
                            if (train.@operator.ToUpper() == "NORTHERN")
                            {
                                NorthernTrains.Add(train);
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {

                if (rClient.CheckConnection("https://huxley.apphb.com/all/wgn/from/liv/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1") == false)
                {
                    outputTextBox.AppendText("\r$$$$$$$$$$$$$$$$$$$$$$$$$$$$ CANNOT CONNECT TO HUXLEY API... \r$$$$$$$$$$$$$$$$$$$$$$$$$$$$");


                }

                else
                {
                    outputTextBox.AppendText(string.Format("\r$$$$$$$$$$$$$$$$$$$$$$$$$$$$ ERROR DESERIALIZING JSON DATA: {0}... \r$$$$$$$$$$$$$$$$$$$$$$$$$$$$", e));
                }


            }
            

        }

        public void FillTrainObj()
        {
            string[] _stationCodes = { "wgn", "pre", "mcv", "wbq", "wac", "mia", "mco" };
            List <string> _stationCodesList = new List<string>();
            _stationCodesList.AddRange(_stationCodes);

            DeserializeJSON(_stationCodesList);
            //InitMockConnection();
        }

        public string DelayedTrainCheck()
        {
            NorthernTrains.OrderBy(train => train.isCancelled ? 0 : 1); //sorts the trains to have the ones cancelled at the front of the list
            string returnStr="";

            if (NorthernTrains.Count != 0)
            {
                foreach (var train in NorthernTrains)
                {
                    if (train.isCancelled == true && train.@operator.ToUpper() == "NORTHERN" && db.CheckServiceID(train.serviceID) == 0) //if train item in the list is marked as cancelled and is not saved in the DB then the msg string will be tweeted.
                    {
                        db.SaveServiceIDs(train.serviceID.ToString());

                        int _ApolTicketNum = db.GetApologyTicketNum();
                        int newApolTicketNum = _ApolTicketNum + 159;
                        db.SaveApologyTicketNum(newApolTicketNum);

                        int oldTotCancellations = db.GetTotalCancelsNum();
                        int newCancellations = oldTotCancellations + 1;
                        db.SaveTotalCancelsNum(newCancellations);


                        string msg = string.Format("\rThe Northern Rail service from {0} to {1}, which was set to arrive at {2}, was cancelled. You owe 159 new apology slips if all the seats where filled. .northernassist", train.origin[0].locationName.ToString(), 
                            train.destination[0].locationName.ToString(), train.sta.ToString());
                        SendTweet(msg, outputTextBox);
                        returnStr += msg;

                    }
                    else
                    {
                        returnStr = string.Format("\rNo more new cancellations detected at {0}.\rTotal Trains: {1} \rApology Ticket Total: {2}\rTotal Recorded Cancellations: {3}", DateTime.Now, NorthernTrains.Count, db.GetApologyTicketNum().ToString(), db.GetTotalCancelsNum());
                    }
                }
            }

            else
            {
                returnStr = string.Format("\rNo trains detected at {0}.\rApology Ticket Total: {1}\rTotal Cancellations: {2}", DateTime.Now, db.GetApologyTicketNum().ToString(), db.GetTotalCancelsNum());
            }
                
            return returnStr;
        }
        /*public void InitMockConnection() //this method reads JSON from a text file rather than from a httprequest. Usual for offline development and for always having a collection member in the trainsDelayed list
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
        }*/

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
