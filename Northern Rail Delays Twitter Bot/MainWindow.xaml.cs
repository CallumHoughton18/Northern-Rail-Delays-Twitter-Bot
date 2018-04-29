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
        public MainWindow()
        {            
            InitializeComponent();
            tweetGenerator.FillTrainObj();
            OutputText.AppendText("\rTotal delayed trains: " + tweetGenerator.delayedNorthernTrains.Count);
        }
        private void GenTweet_Click_1(object sender, RoutedEventArgs e)
        {
            OutputText.AppendText(string.Format("\r\n ----------------------------- \r\n") + tweetGenerator.delayedTrainCheck());
        }


    }
}
