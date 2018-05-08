using System;
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
        public Database()
        {
            dbConnection = new SQLiteConnection("Data source = SavedValues.sqlite3; datetimeformat=CurrentCulture");
            if (!File.Exists("SavedValues.sqlite3"))
            {
                SQLiteConnection.CreateFile("SavedValues.sqlite3");
                Console.WriteLine("db created");
            }
        }

        public void SaveServiceIDs(string trainServiceID)
        {
            OpenConnection();
            string saveQuery = "INSERT INTO SavedTrains('ServiceID') VALUES (@ServiceID)";
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery,dbConnection);
            saveCommand.Parameters.AddWithValue("@ServiceID", trainServiceID);
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
            cmd.CommandText = string.Format("SELECT count(*) FROM SavedTrains WHERE ServiceID='{0}'", serviceID);
            cmd.ExecuteNonQuery();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            CloseConnection();
            return count;
        }

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

        public void SaveCurrentDate(string tableName)
        {
            OpenConnection();
            string currentDate = DateTime.Now.ToShortDateString();
            string saveQuery = string.Format("UPDATE {0} SET Date = '{1}'",tableName, currentDate);
            SQLiteCommand saveCommand = new SQLiteCommand(saveQuery, dbConnection);
            saveCommand.ExecuteNonQuery();
            CloseConnection();
        }



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
    }
}
