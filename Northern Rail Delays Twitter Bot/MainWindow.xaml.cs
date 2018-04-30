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

        public MainWindow()
        {            
            InitializeComponent();
            TweetGenerator.dispatcher = this.Dispatcher;
            tweetGenerator.FillTrainObj();
            OutputText.AppendText("\rTotal delayed trains: " + tweetGenerator.delayedNorthernTrains.Count);
            OutputText.AppendText("\rDate Created: " + tweetGenerator.OriginDate());
            TweetGenerator.outputTextBox = OutputText;
        }
        private void GenTweet_Click_1(object sender, RoutedEventArgs e)
        {
            OutputText.AppendText(string.Format("\r-----------------------------") + tweetGenerator.delayedTrainCheck() + "\r-----------------------------");
        }

        private void GenTweetAuto_Click(object sender, RoutedEventArgs e)
        {
            StartTimer(5);
            OutputText.AppendText(TimerCheckTimeString());
        }

        private void StartTimer(int CycleMinutes)
        {
            dispatcherTimer.Interval = new TimeSpan(0, CycleMinutes, 0);
            dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler(TweetTimer_Tick);
            timeUntilNextCheck = DateTime.Now + dispatcherTimer.Interval;
        }

        private void TweetTimer_Tick(object sender, EventArgs e)
        {
            timeUntilNextCheck = DateTime.Now + dispatcherTimer.Interval;
            OutputText.AppendText(string.Format("\r\n -----------------------------") + tweetGenerator.delayedTrainCheck()+ "\r-----------------------------");
            OutputText.AppendText(TimerCheckTimeString());
        }

        private string TimerCheckTimeString()
        {
            return string.Format("\rTrains cancellations will be checked in: {0}", timeUntilNextCheck);
        }


    }
}
