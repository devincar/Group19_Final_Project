﻿using System;
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
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;

namespace ML04_WPF
{
    /// <summary>
    /// Interaction logic for PlannerPage.xaml
    /// </summary>
    public partial class PlannerPage : Window
    {
        private static string sPath = System.AppDomain.CurrentDomain.BaseDirectory;

        public static class ContractInfo
        {
            public static string OrderID { get; set; }
            public static string Customer { get; set; }
            public static string StartLoc { get; set; }
            public static string EndLoc { get; set; }
        }

        public PlannerPage()
        {
            InitializeComponent();

            string userName = MainWindow.userLogIn.userID;

            userLbl.Content = userName;

            SendOrder.IsEnabled = false;
        }

        private void SendOrder_Click(object sender, RoutedEventArgs e)
        {
            int tripCount = 0;

            if (planetExpress.IsChecked == true)
            {
                tripCount++;
            }
            if (schooners.IsChecked == true)
            {
                tripCount++;
            }
            if (tillmanTransport.IsChecked == true)
            {
                tripCount++;
            }
            if (weHaul.IsChecked == true)
            {
                tripCount++;
            }

            string myConnectionString = MainWindow.userLogIn.myConnectionString;

            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = myConnectionString;
            conn.Open();


            if (orderNum.Text != "")
            {
                try
                {
                    int order = Int32.Parse(orderNum.Text);
                    string sql = "update orders set trip = '" + tripCount + "' where orderID = '" + order + "' and completed = false;";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Close();
                }
                catch (MySqlException ex)
                {

                }

                conn.Close();


                completedBtn_Click(sender, e);


                lblUpdate.Content = "";
                sendUpdate.Content = "Trip Updated";

                conn = new MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();

                string sql2 = "select startLoc from orders where orderID = " + Int32.Parse(orderNum.Text) + ";";
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                MySqlDataReader rdr2 = cmd2.ExecuteReader();

                string startLocation = "";
                string endLocation = "";
                string east = "";
                string kms = "";
                string time = "";

                while (rdr2.Read())
                {
                    var startLoc = rdr2["startLoc"];
                    startLocation = startLoc.ToString();
                }
                rdr2.Close();


                sql2 = "select endLoc from orders where orderID = " + Int32.Parse(orderNum.Text) + ";";
                cmd2 = new MySqlCommand(sql2, conn);
                rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    var endLoc = rdr2["endLoc"];
                    endLocation = endLoc.ToString();
                }
                rdr2.Close();


                sql2 = "select endLoc from orders where orderID = " + Int32.Parse(orderNum.Text) + ";";
                cmd2 = new MySqlCommand(sql2, conn);
                rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    var endLoc = rdr2["endLoc"];
                    endLocation = endLoc.ToString();
                }
                rdr2.Close();

                sql2 = "select east from transportationcorridor where destination = '" + startLocation + "';";
                cmd2 = new MySqlCommand(sql2, conn);
                rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    var easter = rdr2["east"];
                    east = easter.ToString();
                }
                rdr2.Close();


                sql2 = "select kms from transportationcorridor where destination = '" + startLocation + "';";
                cmd2 = new MySqlCommand(sql2, conn);
                rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    var kmss = rdr2["kms"];
                    kms = kmss.ToString();
                }
                rdr2.Close();

                sql2 = "select times from transportationcorridor where destination = '" + startLocation + "';";
                cmd2 = new MySqlCommand(sql2, conn);
                rdr2 = cmd2.ExecuteReader();

                while (rdr2.Read())
                {
                    var times = rdr2["times"];
                    time = times.ToString();
                }
                rdr2.Close();

                if (east != endLocation)
                {

                }

                MessageBox.Show($"Trip from {startLocation} to {endLocation} will be {kms}km and take {time} hours with {tripCount} trips.");
            }
        }
        private void Invoice_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter swExtLogFile = new StreamWriter(sPath + "/AllTimeInvoice.txt", true);
            DataTable dt2 = new DataTable();

            string myConnectionString = MainWindow.userLogIn.myConnectionString;

            dt2 = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(myConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM orders where completed = true;";
                using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    da.Fill(dt2);
            }

            swExtLogFile.Write(Environment.NewLine);
            swExtLogFile.Write("\nAll Time Invoice\n");
            swExtLogFile.Write("Order ID | Customer | Start | End | Trip | Completed\n");
            int i;
            foreach (DataRow row in dt2.Rows)
            {
                object[] array = row.ItemArray;
                for (i = 0; i < array.Length - 1; i++)
                {
                    swExtLogFile.Write(array[i].ToString() + " | ");
                }
                swExtLogFile.WriteLine(array[i].ToString());
            }

            swExtLogFile.Write(DateTime.Now.ToString());
            swExtLogFile.Flush();
            swExtLogFile.Close();
            lblUpdate.Content = "Invoice File Created";
            sendUpdate.Content = "";
        }

        private void completedBtn_Click(object sender, RoutedEventArgs e)
        {
            string myConnectionString;
            myConnectionString = "server=localhost;uid=SQUser;pwd=SQUser;database=mssqdatabase";

            DataTable dt2 = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(myConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM orders where completed = false";
                using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    da.Fill(dt2);
            }
            contractDataTbl.ItemsSource = dt2.DefaultView;
        }

        private void passTime_Click(object sender, RoutedEventArgs e)
        {
            string myConnectionString = MainWindow.userLogIn.myConnectionString;

            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = myConnectionString;
            conn.Open();

            string sql = "update orders set completed = true where trip > 0;";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            conn.Close();
            completedBtn_Click(sender, e);
            lblUpdate.Content = "Time has been passed.";
            sendUpdate.Content = "";
        }

        private void Box_Checked(object sender, RoutedEventArgs e)
        {
            if (planetExpress.IsChecked == true || schooners.IsChecked == true || tillmanTransport.IsChecked == true || weHaul.IsChecked == false)
            {
                SendOrder.IsEnabled = true;
            }
            else
            {
                SendOrder.IsEnabled = false;
            }
        }

        private void supportBtn_Click(object sender, RoutedEventArgs e)
        {
            Email emailPage = new Email();
            emailPage.Show();
        }
    }
}
