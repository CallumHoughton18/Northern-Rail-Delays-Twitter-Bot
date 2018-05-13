using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using Northern_Rail_Delays_Twitter_Bot.Models;
using Northern_Rail_Delays_Twitter_Bot.Controls;
using Northern_Rail_Delays_Twitter_Bot.UserControls;

namespace Northern_Rail_Delays_Twitter_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TwitterHandler tweetGenerator = new TwitterHandler();
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        DateTime timeUntilNextCheck;
        bool timerEnabled = false;
        string greetingMsg = "Welcome back.";

        public MainWindow()
        {            
            InitializeComponent();

            if (Properties.Settings.Default.IsFirstTime)
            {
                tweetGenerator.DeleteAllDbValues();
                greetingMsg = "Welcome new user.";
                Properties.Settings.Default.IsFirstTime = false;
                Properties.Settings.Default.Save();
            }

            CustomOriginDate.mainOutputTxtBox = OutputText;
            DeleteACancellation.mainOutputTxtBox = OutputText;

            TwitterHandler.outputTextBox = OutputText;
            TwitterHandler.dispatcher = this.Dispatcher;
            tweetGenerator.CheckCurrentDate();
            tweetGenerator.FillTrainObj();

            OutputText.AppendText(string.Format("\r{0} Bots origin date: {1}", greetingMsg, tweetGenerator.OriginDate()));
        }

        #region Button Click Event Methods

        private void GenTweet_Click_1(object sender, RoutedEventArgs e)
        {
            tweetGenerator.FillTrainObj();
            OutputText.AppendText(string.Format("\r-----------------------------") + tweetGenerator.CancelledTrainCheck() + "\r-----------------------------");
            OutputText.ScrollToEnd();

        }

        private void GenTweetAuto_Click(object sender, RoutedEventArgs e)
        {

            if(timerEnabled == false)
            {
                StartTimer(5);
                OutputText.AppendText(TimerCheckTimeString());
                GenTweetBtn.IsEnabled = false;
                TotalDelaysandApolTicksBtn.IsEnabled = false;
                TweetTotalDelaysandApolTicksBtn.IsEnabled = false;
                GenTweetAutoBtn.Content = "Stop Auto Generate Tweet";
                timerEnabled = true;
            }

            else
            {
                GenTweetAutoBtn.Content = "Start Auto Generate Tweet";
                GenTweetBtn.IsEnabled = true;
                TotalDelaysandApolTicksBtn.IsEnabled = true;
                TweetTotalDelaysandApolTicksBtn.IsEnabled = true;
                dispatcherTimer.Stop();
                OutputText.AppendText("\rAutomatic train cancellation check stopped.");
                timerEnabled = false;
            }
        }

        private void CheckCancellations_Click(object sender, RoutedEventArgs e)
        {
            OutputText.AppendText(tweetGenerator.GetTotalDelaysAndApols());
            OutputText.ScrollToEnd();
        }

        private void TweetTotalDelaysandApolTicksBtn_Click(object sender, RoutedEventArgs e)
        {
            tweetGenerator.TweetTotalDelaysAndApologyNum();
        }
        #endregion

        #region Timer related methods

        private void StartTimer(int CycleMinutes)
        {
            dispatcherTimer.Interval = new TimeSpan(0, CycleMinutes, 0);
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler(TweetTimer_Tick);
            timeUntilNextCheck = DateTime.Now + dispatcherTimer.Interval;
        }

        private void TweetTimer_Tick(object sender, EventArgs e)
        {
            tweetGenerator.CheckCurrentDate();
            tweetGenerator.FillTrainObj();
            timeUntilNextCheck = DateTime.Now + dispatcherTimer.Interval;
            OutputText.AppendText(string.Format("\n\r-----------------------------") + tweetGenerator.CancelledTrainCheck()+ "\r-----------------------------");
            OutputText.AppendText(TimerCheckTimeString());
            OutputText.ScrollToEnd();
        }

        private string TimerCheckTimeString()
        {
            return string.Format("\rTrains cancellations will be checked at: {0}", timeUntilNextCheck);
        }

        #endregion

        #region toolbar click events

        private void DelAll_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Are you sure you want to erase all saved data?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
            }
            else
            {
                tweetGenerator.DeleteAllDbValues();
                OutputText.AppendText(string.Format("\rAll saved values have been erased. "));
            }
        }

        private void SetCurrDateAsOrg_Click(object sender, RoutedEventArgs e)
        {
            tweetGenerator.SaveCurrentDateAsOrg();
            OutputText.AppendText(string.Format("\rNew Origin Date: {0}", tweetGenerator.OriginDate()));
        }

        private void SetCustomOrgDate_Click(object sender, RoutedEventArgs e)
        {
            new CustomOriginDate().Show();
        }

        private void DelCancels_Click(object sender, RoutedEventArgs e)
        {
            new DeleteACancellation().Show();
        }

        #endregion


    }
}
