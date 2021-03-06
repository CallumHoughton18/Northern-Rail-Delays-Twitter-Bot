﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Globalization;

namespace Northern_Rail_Delays_Twitter_Bot.Models
{
    class Database
    {
        public SQLiteConnection dbConnection;
        public SQLiteDataReader TrainServiceIDs;
        public SQLiteDataReader ApologyTicketNumReader;
        public SQLiteDataReader OriginDateReader;
        public SQLiteDataReader TotalDelaysReader;
        public SQLiteDataReader TwitterAPIKeysReader;
        public Database()
        {
            dbConnection = new SQLiteConnection("Data source = SavedValues.sqlite3; datetimeformat=CurrentCulture");
            if (!File.Exists("SavedValues.sqlite3"))
            {
                SQLiteConnection.CreateFile("SavedValues.sqlite3");

            }
        }

        #region Train rsID methods.

        public void SaveServiceIDs(string trainServiceID)
        {
            OpenConnection();
            string saveQuery = "INSERT INTO SavedTrains('rsID') VALUES (@rsID)";
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery,dbConnection);
            saveCommand.Parameters.AddWithValue("@rsID", trainServiceID);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        public void DeleteAllServiceIDs()
        {
            OpenConnection();
            string deleteQuery = "DELETE FROM SavedTrains";
            SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, dbConnection);
            deleteCommand.ExecuteNonQuery();
            CloseConnection();
        }

        public void GetServiceIDs()
        {
            OpenConnection();
            string getQuery = "SELECT * FROM SavedTrains";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            TrainServiceIDs = getCommand.ExecuteReader();
        }

        public int CheckServiceID(string serviceID)
        {
            OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(dbConnection);
            cmd.CommandText = string.Format("SELECT count(*) FROM SavedTrains WHERE rsID='{0}'", serviceID);
            cmd.ExecuteNonQuery();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            CloseConnection();
            return count;
        }

        #endregion

        #region Apology Ticket Number methods.

        public int GetApologyTicketNum()
        {
            int _apolTicketNum = 0;
            OpenConnection();
            string getQuery = "SELECT * FROM ApologyTicketCounter";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            ApologyTicketNumReader = getCommand.ExecuteReader();
            while(ApologyTicketNumReader.Read())
            {
                _apolTicketNum = ApologyTicketNumReader.GetInt32(0);
            }
            return _apolTicketNum;

        }

        public void SaveApologyTicketNum(int NewApolTicketNum)
        {
            OpenConnection();
            string saveQuery = string.Format("UPDATE ApologyTicketCounter SET NumOfTickets = '{0}'", NewApolTicketNum);
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        public int GetTotalCancelsNum()
        {
            int totalDelays = 0;
            OpenConnection();
            string getQuery = "SELECT * FROM TotalCancels";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            TotalDelaysReader = getCommand.ExecuteReader();
            while (TotalDelaysReader.Read())
            {
                totalDelays = TotalDelaysReader.GetInt32(0);
            }
            return totalDelays;
        }

        public void SaveTotalCancelsNum(int newTotalDelays)
        {
            OpenConnection();
            string saveQuery = string.Format("UPDATE TotalCancels SET NumOfCancels = '{0}'", newTotalDelays);
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        #endregion

        #region Origin Date and Current Date related methods

        public string GetOriginDate()
        {
            OpenConnection();
            string originDateStr = "";
            string getQuery = "SELECT * FROM OriginDate";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            OriginDateReader = getCommand.ExecuteReader();
            while (OriginDateReader.Read())
            {
                originDateStr = OriginDateReader.GetString(0);
            }
            DateTime originDate = DateTime.ParseExact(originDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return originDate.ToShortDateString();
        }

        public string GetSavedDate()
        {
            OpenConnection();
            string originDateStr = "";
            string getQuery = "SELECT * FROM CurrentDate";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            OriginDateReader = getCommand.ExecuteReader();
            while (OriginDateReader.Read())
            {
                originDateStr = OriginDateReader.GetString(0);
            }
            DateTime originDate = DateTime.ParseExact(originDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return originDate.ToShortDateString();
        }

        public void SaveCurrentDate(string tableName, DateTime date)
        {
            OpenConnection();
            string datestr = date.ToShortDateString();
            string saveQuery = string.Format("UPDATE {0} SET Date = '{1}'",tableName, datestr);
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        #endregion

        #region Apology Slip Tweet related methods.
        public int CheckTweets(string tweetIDStr)
        {
            OpenConnection();
            SQLiteCommand cmd = new SQLiteCommand(dbConnection);
            cmd.CommandText = string.Format("SELECT count(*) FROM ApologyTweetIDs WHERE ID='{0}'", tweetIDStr);
            cmd.ExecuteNonQuery();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            CloseConnection();
            return count;
        }

        public void SaveApologyTweetID(string tweetID)
        {
            OpenConnection();
            string saveQuery = "INSERT INTO ApologyTweetIDs('ID') VALUES (@ID)";
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.Parameters.AddWithValue("@ID", tweetID);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }
        #endregion

        #region 'deep' db related methods: this is save data which keeps all of the cancelled trains from every day the program runs.
        public void DeepSaveCancelledTrain(string trainServiceID, string from, string to, string arriveTime, string cancelReason)
        {
            OpenConnection();
            string saveQuery = "INSERT INTO DeepCancelledTrains('rsID', 'From', 'To', 'ArriveTime', 'CancelReason') VALUES (@rsID,@From,@To,@ArriveTime,@CancelReason)";
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.Parameters.AddWithValue("@rsID", trainServiceID);
            saveCommand.Parameters.AddWithValue("@From", from);
            saveCommand.Parameters.AddWithValue("@To", to);
            saveCommand.Parameters.AddWithValue("@ArriveTime", arriveTime);
            saveCommand.Parameters.AddWithValue("@CancelReason", cancelReason);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        public void DeleteDeepSaveCancelledTrains()
        {
            OpenConnection();
            string deleteQuery = "DELETE FROM DeepCancelledTrains";
            SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, dbConnection);
            deleteCommand.ExecuteNonQuery();
            CloseConnection();
            
        }
        #endregion

        public void SetTwitterAPIKeys(Dictionary<string,string> twitterAPIKeys)
        {
            OpenConnection();
            string saveQuery = string.Format("UPDATE TwitterAPIKeys SET CusKey = '{0}', CusKeySecret ='{1}', AssTok = '{2}', AssTokSecret = '{3}'", twitterAPIKeys["cusKey"], twitterAPIKeys["cusKeySecret"], twitterAPIKeys["assTok"], twitterAPIKeys["assTokSecret"]);
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }

        public Dictionary<string,string> GetTwitterAPIKeys()
        {
            OpenConnection();
            Dictionary<string, string> twitterAPIKeys = new Dictionary<string, string>();

            string getQuery = "SELECT * FROM TwitterAPIKeys";
            SQLiteCommand getCommand = new SQLiteCommand(getQuery, dbConnection);
            TwitterAPIKeysReader = getCommand.ExecuteReader();
            while (TwitterAPIKeysReader.Read())
            {
                twitterAPIKeys.Add("cusKey",TwitterAPIKeysReader.GetString(0));
                twitterAPIKeys.Add("cusKeySecret",TwitterAPIKeysReader.GetString(1));
                twitterAPIKeys.Add("assTok",TwitterAPIKeysReader.GetString(2));
                twitterAPIKeys.Add("assTokSecret", TwitterAPIKeysReader.GetString(3));
            }
            CloseConnection();

            return twitterAPIKeys;
        }

        #region your bread and butter open and close db connection methods.
        public void OpenConnection()
        {
            if (dbConnection.State != System.Data.ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }

        public void CloseConnection()
        {
            if (dbConnection.State != System.Data.ConnectionState.Closed)
            {
                dbConnection.Close();
            }
        }
        #endregion
    }
}
