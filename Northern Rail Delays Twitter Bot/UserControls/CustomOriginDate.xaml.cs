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

namespace Northern_Rail_Delays_Twitter_Bot.Controls
{
    /// <summary>
    /// Interaction logic for CustomOriginDate.xaml
    /// </summary>
    public partial class CustomOriginDate : Window
    {
        DateTime selectedDate;
        public static RichTextBox mainOutputTxtBox;

        public CustomOriginDate()
        {
            InitializeComponent();
            OriginDateCal.DisplayDateEnd = DateTime.Today;
        }

        private void OriginDateStr_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CheckIfCalDisplayed();
        }

        private void OriginDateCal_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDate = (DateTime)OriginDateCal.SelectedDate;
            OriginDateStr.Text = selectedDate.ToShortDateString();
            OriginDateCal.Visibility = Visibility.Hidden;
        }

        private void DsplyCal_Click(object sender, RoutedEventArgs e)
        {
            CheckIfCalDisplayed();
        }

        private void SaveDateBtn_Click(object sender, RoutedEventArgs e)
        {

            if (selectedDate != new DateTime())
            {
                Database db = new Database();
                db.SaveCurrentDate("OriginDate", selectedDate);
                mainOutputTxtBox.AppendText(string.Format("\rNew origin date: {0}", selectedDate.ToShortDateString()));
                this.Close();
            }

            else
            {
                MessageBox.Show("You have not selected a date!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void CheckIfCalDisplayed()
        {
            if (OriginDateCal.Visibility == Visibility.Hidden)
            {
                OriginDateCal.Visibility = Visibility.Visible;
            }

            else
            {
                OriginDateCal.Visibility = Visibility.Hidden;
            }
        }
    }
}
