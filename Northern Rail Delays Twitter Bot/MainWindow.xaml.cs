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

namespace Northern_Rail_Delays_Twitter_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TweetGenerator tweetGenerator = new TweetGenerator();
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        DateTime timeUntilNextCheck;
        bool timerEnabled = false;

        public MainWindow()
        {            
            InitializeComponent();
            TweetGenerator.dispatcher = this.Dispatcher;
            tweetGenerator.CheckCurrentDate();
            tweetGenerator.FillTrainObj();
            OutputText.AppendText("\rDate The Bot Was Created: " + tweetGenerator.OriginDate());
            TweetGenerator.outputTextBox = OutputText;
        }

        #region Button Click Event Methods

        private void GenTweet_Click_1(object sender, RoutedEventArgs e)
        {
            OutputText.AppendText(string.Format("\r-----------------------------") + tweetGenerator.DelayedTrainCheck() + "\r-----------------------------");
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
            OutputText.AppendText(string.Format("\n\r-----------------------------") + tweetGenerator.DelayedTrainCheck()+ "\r-----------------------------");
            OutputText.AppendText(TimerCheckTimeString());
            OutputText.ScrollToEnd();
        }

        private string TimerCheckTimeString()
        {
            return string.Format("\rTrains cancellations will be checked at: {0}", timeUntilNextCheck);
        }

        #endregion
    }
}
