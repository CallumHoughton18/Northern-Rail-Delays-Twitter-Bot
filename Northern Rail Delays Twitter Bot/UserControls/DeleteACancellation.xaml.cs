using Northern_Rail_Delays_Twitter_Bot.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Delete_a_Cancellation.xaml
    /// </summary>
    public partial class DeleteACancellation : Window
    {
        Database db = new Database();
        
        //MainWindow related objects, variables, and, values.
        public static RichTextBox mainOutputTxtBox;

        //class level variables.
        int totalApologySlips;
        int totalCancellations;

        public DeleteACancellation()
        {
            InitializeComponent();
            FillComboBox();
        }

        private void FillComboBox()
        {
            int cancelNums = db.GetTotalCancelsNum();
            ObservableCollection<int> cancellations = new ObservableCollection<int>();


            if (cancelNums > 0)
            {
                for (int i = 1; i <= cancelNums; i += 1)
                {
                    cancellations.Add(i);
                }

                CancelNums.ItemsSource = cancellations;
            }

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CancelNums.SelectedItem != null)
            {
                SaveCancelNumsSelection();
                mainOutputTxtBox.AppendText(string.Format("\n\rCancellation deletions saved:\rTotal Cancellations = {0}\rTotal Apology Slips = {1}", totalApologySlips, totalCancellations));
                this.Hide();
            }

            else
            {
                MessageBox.Show("You have not selected the number of cancellations to delete from the dropdown menu!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCancelNumsSelection()
        {
            int numOfCanDeletions = Convert.ToInt32(CancelNums.SelectedItem);
            int _ApolTicketNum = db.GetApologyTicketNum();
            int newApolTicketNum = _ApolTicketNum - (numOfCanDeletions * 159);

            totalApologySlips = newApolTicketNum;

            db.SaveApologyTicketNum(newApolTicketNum);

            int oldTotCancellations = db.GetTotalCancelsNum();
            int newCancellations = oldTotCancellations - numOfCanDeletions;

            totalCancellations = newCancellations;

            db.SaveTotalCancelsNum(newCancellations);
        }
    }
}
