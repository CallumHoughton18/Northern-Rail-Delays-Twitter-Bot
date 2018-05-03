using System;
using System.IO;
using System.Net;

namespace Northern_Rail_Delays_Twitter_Bot
{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        DELETE,
    }
    class RESTclient
    {
        public string endPoint { get; set; }
        public httpVerb httpMethod { get; set; }

        public RESTclient()
        {
            endPoint = string.Empty;
            httpMethod = httpVerb.GET;
        }

        public string makeRequest()
        {

            string strResponseValue = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);

            request.Method = httpMethod.ToString();

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("Error code: " + response.StatusCode.ToString());

                }

                //process response stream (JSON,XML etc)

                using(Stream responseStream = response.GetResponseStream())
                {
                    if(responseStream != null)
                    {
                        using(StreamReader reader = new StreamReader(responseStream))
                        {
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }
            }

            return strResponseValue;
        }

        public bool CheckConnection(String URL)
        {
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return true;
            }

            catch
            {
                return false;
            }

        }
    }
}
