using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    class TwitterHandler
    {
        //Database related objects, variables, and, values.
        Database db = new Database();

        //JSON related objects, variables, and, values.
        RESTclient rClient = new RESTclient();
        public List<jTrains.TrainService> NorthernTrains = new List<jTrains.TrainService>();

        //Tweetsharp related objects, variables, and, values.
        public static Dispatcher dispatcher;
        private  string customer_key;
        private  string customer_key_secret;
        private  string access_token;
        private  string access_token_secret;
        private TwitterService service;
        public TwitterHandler()
        {
            SetTwitterAPIKeys();
        }

        //Image related variables and list to store the path to the apology slip that the bot can send. The list can be expanded to contain more than one image path.
        int currentImageID = 0;
        private static List<string> imageList = new List<string> { Path.Combine(Environment.CurrentDirectory, @"Images\apologyslip.jpg") };

        //MainWindow related objects, variables, and, values.
        public static RichTextBox outputTextBox;

        //A varible which is set to a string to display in the output textbox in the MainWindow to tell the user all db values have been deleted. 
        public string deleteAllStr;

        public void SetTwitterAPIKeys()
        {
            Dictionary<string, string> twitterAPIKeys = new Dictionary<string, string>();
            twitterAPIKeys = db.GetTwitterAPIKeys();
            customer_key = twitterAPIKeys["cusKey"];
            customer_key_secret = twitterAPIKeys["cusKeySecret"];
            access_token = twitterAPIKeys["assTok"];
            access_token_secret = twitterAPIKeys["assTokSecret"];

            service = new TwitterService(customer_key, customer_key_secret, access_token, access_token_secret);
        }

        #region JSON manipulation methods

        private void DeserializeJSON(List<string> StationCodes)
        {

            try
            {
                NorthernTrains.Clear();

                foreach (var stationCode in StationCodes) //duplicate foreach loops with assigment of {0} and {1} flipped: redundant code so could be better implemented 
                {
                    rClient.endPoint = string.Format("https://huxley.apphb.com/all/{0}/from/{1}/5?accessToken=DA1C7740-9DA0-11E4-80E6-A920340000B1", stationCode, "liv");
                    string strResponse = rClient.makeRequest();
                    jTrains gottenjTrains = JsonConvert.DeserializeObject<jTrains>(strResponse);
                    if (gottenjTrains.trainServices != null)
                    {
                        var filteredTrains = gottenjTrains.trainServices.Where(item => gottenjTrains.trainServices.Count(x => x.rsid == item.rsid) < 2);
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
                        var filteredTrains = gottenjTrains.trainServices.Where(item => gottenjTrains.trainServices.Count(x => x.rsid == item.rsid) < 2);
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
                    outputTextBox.AppendText("\r$$$$$$$$$$$$$$$$$$$$$$$$$$$$\rCANNOT CONNECT TO HUXLEY API... \r$$$$$$$$$$$$$$$$$$$$$$$$$$$$");


                }

                else
                {
                    outputTextBox.AppendText(string.Format("\r$$$$$$$$$$$$$$$$$$$$$$$$$$$$\rERROR DESERIALIZING JSON DATA: {0}... \r$$$$$$$$$$$$$$$$$$$$$$$$$$$$", e));
                }


            }


        }

        public void FillTrainObj()
        {
            string[] _stationCodes = { "wgn", "pre", "mcv", "wbq", "wac", "mia", "mco" }; //station codes for the 'end stations' in Liverpool Lime Street train journeys. 
            List<string> _stationCodesList = new List<string>();
            _stationCodesList.AddRange(_stationCodes);

            DeserializeJSON(_stationCodesList);
        }

        public string CancelledTrainCheck()
        {
 
            NorthernTrains.OrderBy(train => train.isCancelled ? 0 : 1); //sorts the trains to have the ones cancelled at the front of the list
            string cancelledTrainsMsg = "";
            string returnStr = "";
            SendApologyTickets();

            if (NorthernTrains.Count != 0)
            {
                foreach (var train in NorthernTrains)
                {
                    if (train.isCancelled == true && train.@operator.ToUpper() == "NORTHERN" && db.CheckServiceID(train.rsid) == 0) //if train item in the list is marked as cancelled and is not saved in the DB then the msg string will be tweeted.
                    {
                        db.SaveServiceIDs(train.rsid.ToString());
                        if (train.cancelReason == null)
                        {
                            train.cancelReason = "";
                        }
                        db.DeepSaveCancelledTrain(train.rsid.ToString(), train.origin[0].locationName.ToString(), train.destination[0].locationName.ToString(), train.sta.ToString(), train.cancelReason.ToString());

                        int _ApolTicketNum = db.GetApologyTicketNum();
                        int newApolTicketNum = _ApolTicketNum + 159;
                        db.SaveApologyTicketNum(newApolTicketNum);

                        int oldTotCancellations = db.GetTotalCancelsNum();
                        int newCancellations = oldTotCancellations + 1;
                        db.SaveTotalCancelsNum(newCancellations);

                        string cancelledTrainTweet = string.Format("\rThe Northern Rail service from {0} to {1}, which was set to arrive at {2}, was cancelled. You owe 159 new apology slips if all the seats where filled. @northernassist @northern_pr", train.origin[0].locationName.ToString(),
                            train.destination[0].locationName.ToString(), train.sta.ToString());
                        cancelledTrainsMsg += cancelledTrainTweet;
                        SendTweet(cancelledTrainTweet, outputTextBox);

                    }
                    else
                    {
                        returnStr = string.Format("\n\rNo more new cancellations detected at {0}.\rTotal Trains: {1} \rApology Ticket Total: {2}\rTotal Recorded Cancellations: {3}", DateTime.Now, NorthernTrains.Count, db.GetApologyTicketNum().ToString(), db.GetTotalCancelsNum());
                    }
                }
            }

            else
            {
                returnStr = string.Format("\rNo trains detected at {0}.\rTotal Apology Slips: {1}\rTotal Cancellations: {2}", DateTime.Now, db.GetApologyTicketNum().ToString(), db.GetTotalCancelsNum());
            }

            return cancelledTrainsMsg + returnStr;
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
                db.SaveCurrentDate("CurrentDate", DateTime.Now);
            }
        }

        public void SaveCurrentDateAsOrg()
        {
            db.SaveCurrentDate("OriginDate", DateTime.Now);
        }

        public void DeleteAllDbValues()
        {

            try
            {
                db.DeleteAllServiceIDs();
                db.SaveApologyTicketNum(0);
                db.SaveTotalCancelsNum(0);
                db.SaveCurrentDate("OriginDate", DateTime.Now);
                db.DeleteDeepSaveCancelledTrains();
                deleteAllStr = "All database values have been reset";

            }

            catch(Exception e)
            {
                deleteAllStr = string.Format("An error has occured while resetting the database values: {0}", e);
                MessageBox.Show(deleteAllStr);

            }

        }

        #endregion

        #region Tweet posting ONLY methods

        public void TweetTotalDelaysAndApologyNum()
        {
            string message = string.Format("Since {0} this bot has detected {1} northern rail cancellations to and from Liverpool Lime Street resulting in {2} apology slips. @northernassist", OriginDate(), db.GetTotalCancelsNum(), db.GetApologyTicketNum());
            SendTweet(message, outputTextBox);
        }

        private void SendApologyTickets()
        {
            List<TwitterStatus> tweets = GetApologyTweets();
            if (tweets.Count != 0)
            {
                foreach (var tweet in tweets)
                {
                    if (db.CheckTweets(tweet.IdStr) == 0)
                    {
                        ReplyMediaTweet(string.Format("This bot is incredibly sorry about the cancellations! @{0}", tweet.User.ScreenName), currentImageID, tweet.Id);
                        db.SaveApologyTweetID(tweet.IdStr);
                        outputTextBox.AppendText(string.Format("\r\n*****************************\rApology Ticket Sent To: @{0} \r*****************************\r", tweet.Text));
                    }
                }
            }
        }

        public void SendTestTweet()
        {
            SendTweet("This is a tweet!", outputTextBox);
        }

        #endregion


        #region TwitterAPIMethods
        private void SendTweet(string _status, RichTextBox OutputTextbox)
        {

            SetTwitterAPIKeys();
            service.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK && _status != "")
                {
                    dispatcher.Invoke(() =>
                    {
                        OutputTextbox.AppendText(string.Format("\r\n*****************************\rTweet sent: {0} \r*****************************\r", _status));
                    });

                }

                else
                {
                    dispatcher.Invoke(() =>
                    {
                        OutputTextbox.AppendText(string.Format("\r\nError sending tweet, statuscode: {0}\rHave you entered the correct API keys?", response.StatusCode));
                    });
                }
            });


        }

        private List<TwitterStatus> GetApologyTweets()
        {
            List<TwitterStatus> resultsList = new List<TwitterStatus>(0);
            var tweets_search = service.Search(new SearchOptions { Q = "#northernrailapologyslip", Resulttype = TwitterSearchResultType.Recent });
            if (tweets_search != null) //tweet_search will return null if the API keys are incorrect. 
            {
                resultsList = new List<TwitterStatus>(tweets_search.Statuses);
            }
            return resultsList;
        }

        private void ReplyMediaTweet(string _status, int imageID, long tweetID) //massive issues here, the JSON reader used by the TweetSharp library only accepts int32 values were as the user value is int64 in Twitters case. This can be bypassed by a try and catch statement but the issue is not being addressed.
        {
            try
            {
                SetTwitterAPIKeys();
                using (var stream = new FileStream(imageList[imageID], FileMode.Open))
                {
                    service.SendTweetWithMedia(new SendTweetWithMediaOptions
                    {
                        InReplyToStatusId = tweetID,
                        Status = _status,
                        Images = new Dictionary<string, Stream> { { imageList[imageID], stream } }
                    }
                    );
                }
            }

            catch
            {
                //bypass the JSON parse error.
            }
        }

        #endregion TwutterAPIMethods
    }
}
