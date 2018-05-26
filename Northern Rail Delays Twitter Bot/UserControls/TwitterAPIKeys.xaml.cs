using Northern_Rail_Delays_Twitter_Bot.Models;
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
using System.Windows.Shapes;

namespace Northern_Rail_Delays_Twitter_Bot.UserControls
{
    /// <summary>
    /// Interaction logic for TwitterApIKeys.xaml
    /// </summary>
    public partial class TwitterAPIKeysWindow : Window
    {
        Database db = new Database();
        Dictionary<string, string> twitterAPIKeys = new Dictionary<string, string>();
        public static RichTextBox mainOutputTxtBox;
        public TwitterAPIKeysWindow()
        {
            InitializeComponent();
            SetTextBoxToTwitAPIKeys();
        }

        private void SetTextBoxToTwitAPIKeys()
        {
            twitterAPIKeys = db.GetTwitterAPIKeys();
            CusKey.Text = twitterAPIKeys["cusKey"];
            CusKeySecret.Text = twitterAPIKeys["cusKeySecret"];
            AssTok.Text = twitterAPIKeys["assTok"];
            AssTokSecret.Text = twitterAPIKeys["assTokSecret"];

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CusKey.Text != "" && CusKeySecret.Text !="" && AssTok.Text != "" && AssTokSecret.Text != "")
            {
                twitterAPIKeys.Clear();
                twitterAPIKeys.Add("cusKey", CusKey.Text.Trim());
                twitterAPIKeys.Add("cusKeySecret", CusKeySecret.Text.Trim());
                twitterAPIKeys.Add("assTok", AssTok.Text.Trim());
                twitterAPIKeys.Add("assTokSecret", AssTokSecret.Text.Trim());
                db.SetTwitterAPIKeys(twitterAPIKeys);
                mainOutputTxtBox.AppendText("\rTwitter API keys have been successfully updated.");
                this.Close();
            }

            else
            {
                MessageBox.Show("One of the API key text boxes does not have a value!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
