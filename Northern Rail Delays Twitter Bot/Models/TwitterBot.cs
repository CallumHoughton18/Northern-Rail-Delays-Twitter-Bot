using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;
using System.Timers;

namespace Northern_Rail_Delays_Twitter_Bot
{
    class TwitterBot
    {
        private static string customer_key = "";
        private static string customer_key_secret = "";
        private static string access_token = "";
        private static string access_token_secret = "";

        private static TwitterService service = new TwitterService(customer_key_secret, access_token, access_token_secret);

        private static void SendTweet(string _status)
        {
            service.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Return = Tweet sent! to textviewer
                }

                else
                {
                    //Return  = Error sending tweet
                }
            });
        }
    }
}
