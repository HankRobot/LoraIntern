using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace LoraIntern
{
    public sealed partial class VisualData : Page
    {
        public int start = 0; //the first row to start iterate
        public int end = 59;  //the last row after iteration
        public int norows;    //total number of rows in the server
        public DateTime empty; //leave this empty for certain functions
        public bool filepicked; //to check if csv file is selected by user

        public VisualData()
        {
            this.InitializeComponent();
            readnumberofrows();
            DateTime today = DateTime.Now.Date;
            CurrentDate.Text = today.ToShortDateString();
            Debug.WriteLine("This is the date today!",today.ToString(""));
            findsqlDate(today);
        }

        //number of rows the SQL table currently has
        public void readnumberofrows()
        {
            string retrieve = "SELECT COUNT(*) FROM LORA_TABLE";

            SqlConnectionStringBuilder sql = LoraSQLConnect.ConnectionString();

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(retrieve, sqlConn);
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            norows = reader.GetInt32(0);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    LoraSQLConnect.DisplaySqlErrors(ex,true);
                }
            }
        }

        //load the graphs of sensor data collected
        public void LoadChartContents()
        {
            string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};",start,end);

            var loradata = LoraSQLConnect.GetLoraDatabaseData(retrieve,false,empty,0,0);
       
            var hankrecords = loradata.Item1;
            var lorarecords = loradata.Item2;
            CurrentDate.Text = loradata.Item5;
            
            (dustChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.dust).ToList();
            (uvChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.uv).ToList(); 
            (temperatureChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.temperature).ToList(); 
            (pressureChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.pressure).ToList(); 
            (humidityChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.humidity).ToList(); 
            (RSSIChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.rssi).ToList(); 

            (dustChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.dust).ToList(); 
            (uvChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.uv).ToList(); 
            (temperatureChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.temperature).ToList(); 
            (pressureChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.pressure).ToList(); 
            (humidityChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.humidity).ToList(); 
            (RSSIChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.rssi).ToList(); 
        }
        
        //function for next page button for next extra sets of sensor data to display
        //on graph
        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            readnumberofrows();
            if (end<norows)
            {
                start += 59;
                end += 59;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("No more data to load!");
                await popup.ShowAsync();
            }
        }

        //back page button
        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            if (start>0)
            {
                start -= 59;
                end -= 59;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("You have reached the end!");
                await popup.ShowAsync();
            }
        }

        //function for searching for date and time in the sql date
        private async void SearchDate_Click(object sender, RoutedEventArgs e)
        {
            dialog.Title = "Search for a DateTime";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";

            await dialog.ShowAsync();

            var date = datepicker.Date;
            string time = timepicker.Time.ToString();
            string formateddate;

            if (date!=null && time!=null)
            {
                DateTime datesl = date.Value.DateTime;
                formateddate = datesl.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                if ((formateddate + " " + time) != "")
                {
                    try
                    {
                        DateTime sqlDatetime = Convert.ToDateTime(formateddate + " " + time);
                        Debug.WriteLine(sqlDatetime.ToString(), "It worked!");
                        findsqlDate(sqlDatetime);
                    }
                    catch (Exception ex)
                    {
                        MessageDialog popup = new MessageDialog(ex.ToString(), "Wrong DateTime Format");
                        await popup.ShowAsync();
                    }
                }
            }
        }

        //function for search date and retrieve sensor data from server
        private void findsqlDate(DateTime date)
        {
            start = 0;
            end = 59;
            object lastrow = 0;
            int counter = 0;

            string retrieve = String.Format("SET ROWCOUNT 60; select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                    "as Sub where TimeSubmit >= '{0}';", date.ToString("MM-dd-yyyy HH:mm:ss"));
            Debug.WriteLine("interesting", retrieve);

            var loradata = LoraSQLConnect.GetLoraDatabaseData(retrieve, true, date, counter, lastrow);

            var hankrecords = loradata.Item1;
            var lorarecords = loradata.Item2;
            CurrentDate.Text = loradata.Item5;

            counter = loradata.Item3;
            lastrow = loradata.Item4;

            start += Convert.ToInt32(lastrow)-counter+1;
            end += Convert.ToInt32(lastrow)-counter+1;
            
            (dustChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.dust).ToList();
            (uvChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.uv).ToList();
            (temperatureChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.temperature).ToList();
            (pressureChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.pressure).ToList();
            (humidityChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.humidity).ToList();
            (RSSIChart.Series[0] as LineSeries).ItemsSource = hankrecords.SelectMany(i => i.rssi).ToList();

            (dustChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.dust).ToList();
            (uvChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.uv).ToList();
            (temperatureChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.temperature).ToList();
            (pressureChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.pressure).ToList();
            (humidityChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.humidity).ToList();
            (RSSIChart.Series[1] as LineSeries).ItemsSource = lorarecords.SelectMany(i => i.rssi).ToList();
        }

        //function for going back to main page
        private void Connection_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoraIntern.MainPage));
        }

        //function for the save to csv file
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ProgressSave.IsActive = true;

            MessageDialog dialog = new MessageDialog("Saving Lora Datasets...Press ESC to cancel");
            dialog.Title = "Saving Lora Datasets...";
            dialog.Commands.Add(new UICommand("Download This Page Only", null));
            dialog.Commands.Add(new UICommand("Download Between Dates", null));
            dialog.Commands.Add(new UICommand("Download the entire database", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Download This Page Only")
            {
                string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", start, end);
                await GetLogging.DownloadCSV(retrieve);
            }

            if (cmd.Label == "Download the entire database")
            {
                string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);
                await GetLogging.DownloadCSV(retrieve);
            }

            if (cmd.Label == "Download Between Dates")
            {
                dialogsave.Title = "Download between Dates";
                
                dialogsave.IsSecondaryButtonEnabled = true;
                dialogsave.PrimaryButtonText = "Ok";
                dialogsave.SecondaryButtonText = "Cancel";

                await dialogsave.ShowAsync();

                var fdate = fromdate.Date;
                var tdate = todate.Date;

                if (fdate!=null && tdate!=null)
                {
                    DateTime datesl = fdate.Value.DateTime;
                    string ffdate = datesl.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                    
                    DateTime dates2 = tdate.Value.DateTime;
                    string ttdate = dates2.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                    
                    if (ffdate != "" && ttdate != "")
                    {
                        try
                        {
                            DateTime date1 = Convert.ToDateTime(ffdate);
                            DateTime date2 = Convert.ToDateTime(ttdate);
                            Debug.WriteLine(date1, "It worked!1");
                            Debug.WriteLine(date2, "It worked!2");

                            string retrieve = string.Format("select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                                                            "as Sub where TimeSubmit >= '{0}' and TimeSubmit <='{1}';",
                                                            date1.ToString("MM-dd-yyyy 00:00:00"), date2.ToString("MM-dd-yyyy 23:59:59"));

                            await GetLogging.DownloadCSV(retrieve);
                        }
                        catch (Exception ex)
                        {
                            MessageDialog popup = new MessageDialog(ex.ToString(), "Wrong DateTime Format");
                            await popup.ShowAsync();
                        }
                    }
                }
            }

            ProgressSave.IsActive = false;
        }
    }
}
